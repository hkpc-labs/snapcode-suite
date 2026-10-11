// ============================================================
// Firebase Configuration — SnapCode
// ============================================================

import { initializeApp } from 'firebase/app';
import { getAuth } from 'firebase/auth';

const firebaseConfig = {
  apiKey: "AIzaSyC40xlFduuZNWd7JfjtBw6cDiLbvUNS8Wg",
  authDomain: "rdclub-f9e66.firebaseapp.com",
  databaseURL: "https://rdclub-f9e66-default-rtdb.firebaseio.com",
  projectId: "rdclub-f9e66",
  storageBucket: "rdclub-f9e66.firebasestorage.app",
  messagingSenderId: "172474979134",
  appId: "1:172474979134:web:eebfc160096401e896926f",
  measurementId: "G-WS41CK6XGS"
};

const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);
export default app;
