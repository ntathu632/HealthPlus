import { Component, Inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DatePipe } from '@angular/common';
import { MedicalHistoryService } from '../../../core/services/medical-history.service';
import { MedicalHistory } from '../../../models/medical-history.models';

@Component({
    templateUrl: './medical-history-detail-dialog.component.html',
    styleUrl: './medical-history-detail-dialog.component.scss',
    selector: 'app-medical-history-detail-dialog',
  standalone: true,
  imports: [
    DatePipe,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatDividerModule, MatProgressBarModule, MatTooltipModule,
  ]
})
export class MedicalHistoryDetailDialogComponent {
  uploading    = signal(false);
  uploadingFile= signal('');
  uploadedDocs = signal<any[]>([]);

  constructor(
    private svc: MedicalHistoryService,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public item: MedicalHistory,
  ) {}

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      this.snack.open('File tối đa 10MB', 'Đóng', { duration: 3000 });
      return;
    }

    this.uploading.set(true);
    this.uploadingFile.set(file.name);

    this.svc.uploadDocument(this.item.id, file).subscribe({
      next: res => {
        this.uploadedDocs.update(docs => [...docs, res.data]);
        this.uploading.set(false);
        this.snack.open('Upload thành công', 'Đóng', { duration: 2000, panelClass: 'snack-success' });
      },
      error: () => {
        this.uploading.set(false);
        this.snack.open('Upload thất bại', 'Đóng', { duration: 3000 });
      },
    });
  }

  isUpcoming(): boolean {
    if (!this.item.followUpDate) return false;
    const days = this.daysUntil();
    return days >= 0 && days <= 30;
  }

  daysUntil(): number {
    const diff = new Date(this.item.followUpDate!).getTime() - new Date().setHours(0,0,0,0);
    return Math.ceil(diff / 86400000);
  }

  getDocIcon(fileType: string): string {
    if (fileType?.includes('pdf'))   return 'picture_as_pdf';
    if (fileType?.includes('image')) return 'image';
    return 'insert_drive_file';
  }

  formatSize(bytes: number): string {
    if (bytes < 1024)        return `${bytes} B`;
    if (bytes < 1048576)     return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  getDocUrl(fileUrl: string): string {
    return fileUrl?.startsWith('http') ? fileUrl : `http://localhost:5146${fileUrl}`;
  }
}
