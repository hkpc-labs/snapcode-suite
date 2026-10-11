import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './AuthPages.css';

export default function RegisterPage() {
 const [displayName, setDisplayName] = useState('');
 const [email, setEmail] = useState('');
 const [password, setPassword] = useState('');
 const [confirmPassword, setConfirmPassword] = useState('');
 const [error, setError] = useState('');
 const [loading, setLoading] = useState(false);

 const { register } = useAuth();
 const navigate = useNavigate();

 async function handleSubmit(e) {
 e.preventDefault();
 setError('');

 if (password.length < 6) {
 setError('Password must be at least 6 characters.');
 return;
 }
 if (password !== confirmPassword) {
 setError('Passwords do not match.');
 return;
 }
 if (!displayName.trim()) {
 setError('Display name is required.');
 return;
 }

 setLoading(true);
 try {
 await register(email, password, displayName.trim());
 navigate('/dashboard');
 } catch (err) {
 const msg = err.code === 'auth/email-already-in-use'
 ? 'This email is already registered. Try logging in.'
 : err.code === 'auth/weak-password'
 ? 'Password is too weak. Use at least 6 characters.'
 : 'Registration failed. Please try again.';
 setError(msg);
 } finally {
 setLoading(false);
 }
 }

 return (
 <div className="auth-page">
 <div className="auth-container animate-fade-in">
 <div className="auth-brand">
 <div className="auth-logo">
 <span className="logo-icon"></span>
 <span className="logo-text">SnapCode</span>
 </div>
 <p className="auth-subtitle">Start building apps with blocks</p>
 </div>

 <div className="auth-card glass-card-strong">
 <h2 className="auth-title">Create account</h2>
 <p className="auth-desc text-secondary">Join SnapCode for free</p>

 {error && (
 <div className="auth-error animate-shake">
 <span></span> {error}
 </div>
 )}

 <form onSubmit={handleSubmit}>
 <div className="input-group">
 <label className="input-label" htmlFor="reg-name">Display Name</label>
 <input
 id="reg-name"
 type="text"
 className="input-field"
 placeholder="Hardik Prajapati"
 value={displayName}
 onChange={(e) => setDisplayName(e.target.value)}
 required
 autoComplete="name"
 />
 </div>

 <div className="input-group">
 <label className="input-label" htmlFor="reg-email">Email</label>
 <input
 id="reg-email"
 type="email"
 className="input-field"
 placeholder="you@university.edu"
 value={email}
 onChange={(e) => setEmail(e.target.value)}
 required
 autoComplete="email"
 />
 </div>

 <div className="input-group">
 <label className="input-label" htmlFor="reg-password">Password</label>
 <input
 id="reg-password"
 type="password"
 className="input-field"
 placeholder="••••••••"
 value={password}
 onChange={(e) => setPassword(e.target.value)}
 required
 minLength={6}
 autoComplete="new-password"
 />
 <p className="password-requirements">Minimum 6 characters</p>
 </div>

 <div className="input-group">
 <label className="input-label" htmlFor="reg-confirm">Confirm Password</label>
 <input
 id="reg-confirm"
 type="password"
 className={`input-field ${password && confirmPassword && password !== confirmPassword ? 'input-error' : ''}`}
 placeholder="••••••••"
 value={confirmPassword}
 onChange={(e) => setConfirmPassword(e.target.value)}
 required
 autoComplete="new-password"
 />
 {password && confirmPassword && password !== confirmPassword && (
 <p className="error-text">Passwords don't match</p>
 )}
 </div>

 <button
 type="submit"
 className="btn btn-primary w-full btn-lg"
 disabled={loading}
 >
 {loading ? <span className="spinner"></span> : 'Create Account'}
 </button>
 </form>

 <div className="auth-divider">
 <span>Already have an account?</span>
 </div>

 <Link to="/login" className="btn btn-secondary w-full">
 Sign in instead
 </Link>
 </div>
 </div>
 </div>
 );
}
