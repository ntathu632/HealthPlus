import { Component, OnInit, signal, computed, Inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';
import { ReminderService } from '../../../core/services/reminder.service';
import { PushNotificationService } from '../../../core/services/push-notification.service';
import {
  Reminder, ReminderType, ReminderChannel, RepeatType,
  CreateReminderRequest, UpdateReminderRequest,
  NotificationSetting, UpdateNotificationSettingRequest,
} from '../../../models/reminder.models';

// ─── Notification Settings Dialog ────────────────────────────────────────────

@Component({
    templateUrl: './notif-settings-dialog.component.html',
    styleUrl: './notif-settings-dialog.component.scss',
    selector: 'app-notif-settings-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSlideToggleModule,
    MatDividerModule,
  ]
})
export class NotifSettingsDialogComponent implements OnInit {
  form!: FormGroup;
  loadingSettings = signal(true);
  saving = signal(false);
  private currentFcmToken?: string;

  constructor(
    private fb: FormBuilder,
    private svc: ReminderService,
    protected pushService: PushNotificationService,
    private dialogRef: MatDialogRef<NotifSettingsDialogComponent>,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      pushEnabled:              [true],
      emailEnabled:             [false],
      smsEnabled:               [false],
      vaccineReminderEnabled:   [true],
      medicineReminderEnabled:  [true],
      followUpReminderEnabled:  [true],
      quietHourStart:           [''],
      quietHourEnd:             [''],
    });
    this.svc.getSettings().subscribe({
      next: res => {
        const s = res.data;
        this.currentFcmToken = s.fcmToken;
        this.form.patchValue({
          pushEnabled:             s.pushEnabled,
          emailEnabled:            s.emailEnabled,
          smsEnabled:              s.smsEnabled,
          vaccineReminderEnabled:  s.vaccineReminderEnabled,
          medicineReminderEnabled: s.medicineReminderEnabled,
          followUpReminderEnabled: s.followUpReminderEnabled,
          quietHourStart: s.quietHourStart ? s.quietHourStart.substring(0, 5) : '',
          quietHourEnd:   s.quietHourEnd   ? s.quietHourEnd.substring(0, 5)   : '',
        });
        this.loadingSettings.set(false);
      },
      error: () => this.loadingSettings.set(false),
    });
  }

  save(): void {
    this.saving.set(true);
    const v = this.form.value;

    const submit = (fcmToken?: string) => {
      const payload: UpdateNotificationSettingRequest = {
        pushEnabled:             v.pushEnabled,
        emailEnabled:            v.emailEnabled,
        smsEnabled:              v.smsEnabled,
        vaccineReminderEnabled:  v.vaccineReminderEnabled,
        medicineReminderEnabled: v.medicineReminderEnabled,
        followUpReminderEnabled: v.followUpReminderEnabled,
        quietHourStart: v.quietHourStart || undefined,
        quietHourEnd:   v.quietHourEnd   || undefined,
        fcmToken,
      };
      this.svc.updateSettings(payload).subscribe({
        next: () => {
          this.snack.open('Đã lưu cài đặt thông báo', 'Đóng', { duration: 2500 });
          this.dialogRef.close(true);
        },
        error: () => this.saving.set(false),
      });
    };

    if (v.pushEnabled && this.pushService.isConfigured && this.pushService.isSupported) {
      this.pushService.requestPermissionAndGetToken().then(token => submit(token ?? this.currentFcmToken));
    } else {
      submit(this.currentFcmToken);
    }
  }
}

// ─── Reminder Form Dialog ─────────────────────────────────────────────────────

interface ReminderFormData { reminder: Reminder | null; }

@Component({
    templateUrl: './reminder-form-dialog.component.html',
    styleUrl: './reminder-form-dialog.component.scss',
    selector: 'app-reminder-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSlideToggleModule,
  ]
})
export class ReminderFormDialogComponent {
  form: FormGroup;
  loading = signal(false);
  isEdit: boolean;

  constructor(
    fb: FormBuilder,
    private svc: ReminderService,
    private dialogRef: MatDialogRef<ReminderFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: ReminderFormData,
  ) {
    this.isEdit = !!data.reminder;
    const r = data.reminder;
    this.form = fb.group({
      title:        [r?.title ?? '',         Validators.required],
      reminderType: [r?.reminderType ?? 'Medicine'],
      channel:      [r?.channel ?? 'Push'],
      remindAt:     [r ? toLocalDatetimeInput(r.remindAt) : defaultRemindAt(), Validators.required],
      repeatType:   [r?.repeatType ?? 'None'],
      repeatUntil:  [r?.repeatUntil ?? ''],
      message:      [r?.message ?? ''],
      isEnabled:    [r?.isEnabled ?? true],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    const v = this.form.value;

    const obs = this.isEdit
      ? this.svc.update(this.data.reminder!.id, {
          title:       v.title,
          message:     v.message || undefined,
          remindAt:    new Date(v.remindAt).toISOString(),
          isEnabled:   v.isEnabled,
          channel:     v.channel as ReminderChannel,
          repeatType:  v.repeatType !== 'None' ? v.repeatType as RepeatType : undefined,
          repeatUntil: v.repeatUntil || undefined,
        } satisfies UpdateReminderRequest)
      : this.svc.create({
          reminderType: v.reminderType as ReminderType,
          title:        v.title,
          message:      v.message || undefined,
          remindAt:     new Date(v.remindAt).toISOString(),
          channel:      v.channel as ReminderChannel,
          repeatType:   v.repeatType !== 'None' ? v.repeatType as RepeatType : undefined,
          repeatUntil:  v.repeatUntil || undefined,
        } satisfies CreateReminderRequest);

    obs.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật' : 'Tạo nhắc nhở thành công', 'Đóng',
          { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}

function toLocalDatetimeInput(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function defaultRemindAt(): string {
  const d = new Date();
  d.setHours(d.getHours() + 1, 0, 0, 0);
  return toLocalDatetimeInput(d.toISOString());
}

// ─── Reminders List (main page) ───────────────────────────────────────────────

@Component({
    templateUrl: './reminders-list.component.html',
    styleUrl: './reminders-list.component.scss',
    selector: 'app-reminders-list',
  standalone: true,
  imports: [
    DatePipe, NgClass,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule, MatSlideToggleModule,
  ]
})
export class RemindersListComponent implements OnInit {
  allItems       = signal<Reminder[]>([]);
  upcoming       = signal<Reminder[]>([]);
  loading        = signal(true);
  upcomingLoading = signal(true);
  selectedFilter = 'All';
  selectedType   = 'All';

  readonly filterTabs = [
    { value: 'All',     label: 'Tất cả' },
    { value: 'Active',  label: 'Đang bật' },
    { value: 'Sent',    label: 'Đã gửi' },
    { value: 'Disabled',label: 'Đã tắt' },
  ];

  readonly typeTabs = [
    { value: 'All',      label: 'Tất cả',    icon: 'list' },
    { value: 'Medicine', label: 'Thuốc',     icon: 'medication' },
    { value: 'Vaccine',  label: 'Vaccine',   icon: 'vaccines' },
    { value: 'FollowUp', label: 'Tái khám',  icon: 'event_note' },
    { value: 'Checkup',  label: 'Khám định kỳ', icon: 'monitor_heart' },
  ];

  filteredItems = computed(() => {
    let items = this.allItems();
    if (this.selectedFilter === 'Active')   items = items.filter(r => r.isEnabled && !r.isSent);
    if (this.selectedFilter === 'Sent')     items = items.filter(r => r.isSent);
    if (this.selectedFilter === 'Disabled') items = items.filter(r => !r.isEnabled);
    if (this.selectedType !== 'All')        items = items.filter(r => r.reminderType === this.selectedType);
    return items.sort((a, b) => new Date(a.remindAt).getTime() - new Date(b.remindAt).getTime());
  });

  constructor(
    private svc: ReminderService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void { this.load(); this.loadUpcoming(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next: res => { this.allItems.set(res.data); this.loading.set(false); },
      error: ()  => this.loading.set(false),
    });
  }

  loadUpcoming(): void {
    this.upcomingLoading.set(true);
    this.svc.getUpcoming(24).subscribe({
      next: res => { this.upcoming.set(res.data); this.upcomingLoading.set(false); },
      error: ()  => this.upcomingLoading.set(false),
    });
  }

  countByFilter(filter: string): number {
    const all = this.allItems();
    if (filter === 'All')      return all.length;
    if (filter === 'Active')   return all.filter(r => r.isEnabled && !r.isSent).length;
    if (filter === 'Sent')     return all.filter(r => r.isSent).length;
    if (filter === 'Disabled') return all.filter(r => !r.isEnabled).length;
    return 0;
  }

  openCreate(): void {
    this.dialog.open(ReminderFormDialogComponent, {
      width: '540px',
      data: { reminder: null } satisfies ReminderFormData,
    }).afterClosed().subscribe(ok => { if (ok) { this.load(); this.loadUpcoming(); } });
  }

  openEdit(r: Reminder): void {
    this.dialog.open(ReminderFormDialogComponent, {
      width: '540px',
      data: { reminder: r } satisfies ReminderFormData,
    }).afterClosed().subscribe(ok => { if (ok) { this.load(); this.loadUpcoming(); } });
  }

  openSettings(): void {
    this.dialog.open(NotifSettingsDialogComponent, { width: '460px' })
      .afterClosed().subscribe();
  }

  toggleEnabled(r: Reminder, enabled: boolean): void {
    const req: UpdateReminderRequest = {
      title:      r.title,
      message:    r.message,
      remindAt:   r.remindAt,
      isEnabled:  enabled,
      channel:    r.channel,
      repeatType: r.repeatType !== 'None' ? r.repeatType : undefined,
      repeatUntil: r.repeatUntil,
    };
    this.svc.update(r.id, req).subscribe({
      next: res => {
        this.allItems.update(cur => cur.map(x => x.id === r.id ? res.data : x));
        this.snack.open(enabled ? 'Đã bật nhắc nhở' : 'Đã tắt nhắc nhở', 'Đóng', { duration: 2000 });
      },
    });
  }

  delete(r: Reminder): void {
    if (!confirm(`Xoá nhắc nhở "${r.title}"?`)) return;
    this.svc.delete(r.id).subscribe({
      next: () => {
        this.snack.open('Đã xoá', 'Đóng', { duration: 2000 });
        this.allItems.update(cur => cur.filter(x => x.id !== r.id));
        this.upcoming.update(cur => cur.filter(x => x.id !== r.id));
      },
    });
  }

  typeIcon(t: ReminderType): string {
    const map: Record<ReminderType, string> = {
      Medicine: 'medication', Vaccine: 'vaccines', FollowUp: 'event_note', Checkup: 'monitor_heart',
    };
    return map[t] ?? 'notifications';
  }

  channelIcon(c: ReminderChannel): string {
    return c === 'Push' ? 'notifications' : c === 'Email' ? 'email' : 'sms';
  }

  channelLabel(c: ReminderChannel): string {
    return c === 'Push' ? 'Push' : c === 'Email' ? 'Email' : 'SMS';
  }

  repeatLabel(r: RepeatType | undefined): string {
    return r === 'Daily' ? 'Hàng ngày' : r === 'Weekly' ? 'Hàng tuần' : '';
  }

  isPast(dateStr: string): boolean {
    return new Date(dateStr).getTime() < Date.now();
  }

  isSoon(dateStr: string): boolean {
    const diff = new Date(dateStr).getTime() - Date.now();
    return diff > 0 && diff < 3 * 60 * 60 * 1000;
  }

  countdownLabel(dateStr: string): string {
    const diff = new Date(dateStr).getTime() - Date.now();
    if (diff < 0) {
      const m = Math.floor(-diff / 60000);
      return m < 60 ? `${m} phút trước` : `${Math.floor(m/60)} giờ trước`;
    }
    const m = Math.floor(diff / 60000);
    if (m < 60) return `còn ${m} phút`;
    const h = Math.floor(m / 60);
    if (h < 24) return `còn ${h} giờ`;
    return `còn ${Math.floor(h/24)} ngày`;
  }
}
