export type ReminderType = 'Vaccine' | 'Medicine' | 'FollowUp' | 'Checkup';
export type ReminderChannel = 'Push' | 'Email' | 'Sms';
export type RepeatType = 'None' | 'Daily' | 'Weekly';

export interface Reminder {
  id: string;
  userId: string;
  reminderType: ReminderType;
  relatedEntityId?: string;
  relatedEntityType?: string;
  title: string;
  message?: string;
  remindAt: string;
  isEnabled: boolean;
  isSent: boolean;
  sentAt?: string;
  channel: ReminderChannel;
  repeatType?: RepeatType;
  repeatUntil?: string;
  createdAt: string;
}

export interface CreateReminderRequest {
  reminderType: ReminderType;
  relatedEntityId?: string;
  relatedEntityType?: string;
  title: string;
  message?: string;
  remindAt: string;
  channel: ReminderChannel;
  repeatType?: RepeatType;
  repeatUntil?: string;
}

export interface UpdateReminderRequest {
  title: string;
  message?: string;
  remindAt: string;
  isEnabled: boolean;
  channel: ReminderChannel;
  repeatType?: RepeatType;
  repeatUntil?: string;
}

export interface NotificationSetting {
  userId: string;
  pushEnabled: boolean;
  emailEnabled: boolean;
  smsEnabled: boolean;
  vaccineReminderEnabled: boolean;
  medicineReminderEnabled: boolean;
  followUpReminderEnabled: boolean;
  quietHourStart?: string;
  quietHourEnd?: string;
  fcmToken?: string;
  updatedAt: string;
}

export interface UpdateNotificationSettingRequest {
  pushEnabled: boolean;
  emailEnabled: boolean;
  smsEnabled: boolean;
  vaccineReminderEnabled: boolean;
  medicineReminderEnabled: boolean;
  followUpReminderEnabled: boolean;
  quietHourStart?: string;
  quietHourEnd?: string;
  fcmToken?: string;
}
