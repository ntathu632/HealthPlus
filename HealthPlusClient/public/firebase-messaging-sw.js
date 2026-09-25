importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-messaging-compat.js');

// Phải khớp với environment.firebaseConfig (src/environments/environment*.ts) —
// service worker chạy ngoài Angular nên không import được environment.ts trực tiếp.
firebase.initializeApp({
  apiKey: 'AIzaSyDFjdvUdaUDBjPqFpMLRddAbRoer8RIWOs',
  authDomain: 'healthplus-6a73f.firebaseapp.com',
  projectId: 'healthplus-6a73f',
  storageBucket: 'healthplus-6a73f.firebasestorage.app',
  messagingSenderId: '852027992399',
  appId: '1:852027992399:web:02528d93cd8a852685fd3a',
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  const title = payload.notification?.title ?? 'HealthPlus';
  const body = payload.notification?.body ?? '';
  self.registration.showNotification(title, {
    body,
    icon: '/favicon.ico',
  });
});
