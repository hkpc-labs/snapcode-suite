import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './AuthPages.css';

export default function LoginPage() {
 const [email, setEmail] = useState('');
 const [password, setPassword] = useState('');
 const [error, setError] = useState('');
 const [loading, setLoading] = useState(false);
 const [resetSent, setResetSent] = useState(false);
 const [showReset, setShowReset] = useState(false);

 const { login, resetPassword } = useAuth();
 const navigate = useNavigate();

 async function handleSubmit(e) {
 e.preventDefault();
 setError('');
 setLoading(true);

 try {
 await login(email, password);
 navigate('/dashboard');
 } catch (err) {
 const msg = err.code === 'auth/invalid-credential'
 ? 'Invalid email or password.'
 : err.code === 'auth/too-many-requests'
 ? 'Too many attempts. Please try again later.'
 : 'Login failed. Please try again.';
 setError(msg);
 } finally {
 setLoading(false);
 }
 }

 async function handleResetPassword(e) {
 e.preventDefault();
 if (!email) {
 setError('Enter your email above to reset password.');
 return;
 }
 setLoading(true);
 try {
 await resetPassword(email);
 setResetSent(true);
 setError('');
 } catch {
 setError('Could not send reset email. Check your email address.');
 } finally {
 setLoading(false);
 }
 }

 return (
 <div className="auth-page">
 <div className="auth-container animate-fade-in">
 {/* Brand */}
 <div className="auth-brand">
 <div className="auth-logo">
 <span className="logo-icon"></span>
 <span className="logo-text">SnapCode</span>
 </div>
 <p className="auth-subtitle">Visual Block Coding → Flutter Apps</p>
 </div>

 {/* Card */}
 <div className="auth-card glass-card-strong">
 <h2 className="auth-title">Welcome back</h2>
 <p className="auth-desc text-secondary">Sign in to continue building</p>

 {error && (
 <div className="auth-error animate-shake">
 <span></span> {error}
 </div>
 )}

 {resetSent && (
 <div className="auth-success">
 <span></span> Password reset email sent! Check your inbox.
 </div>
 )}

 <form onSubmit={handleSubmit}>
 <div className="input-group">
 <label className="input-label" htmlFor="login-email">Email</label>
 <input
 id="login-email"
 type="email"
 className="input-field"
 placeholder="you@university.edu"
 value={email}
 onChange={(e) => setEmail(e.target.value)}
 required
 autoComplete="email"
 />
 </div>

 {!showReset && (
 <div className="input-group">
 <label className="input-label" htmlFor="login-password">Password</label>
 <input
 id="login-password"
 type="password"
 className="input-field"
 placeholder="••••••••"
 value={password}
 onChange={(e) => setPassword(e.target.value)}
 required
 autoComplete="current-password"
 />
 </div>
 )}

 {showReset ? (
 <>
 <button
 type="button"
 className="btn btn-primary w-full"
 onClick={handleResetPassword}
 disabled={loading}
 >
 {loading ? <span className="spinner"></span> : 'Send Reset Link'}
 </button>
 <button
 type="button"
 className="btn btn-ghost w-full mt-sm"
 onClick={() => { setShowReset(false); setResetSent(false); }}
 >
 ← Back to login
 </button>
 </>
 ) : (
 <>
 <button
 type="submit"
 className="btn btn-primary w-full btn-lg"
 disabled={loading}
 >
 {loading ? <span className="spinner"></span> : 'Sign In'}
 </button>

 <button
 type="button"
 className="auth-forgot-link btn btn-ghost w-full"
 onClick={() => setShowReset(true)}
 >
 Forgot password?
 </button>
 </>
 )}
 </form>

 <div className="auth-divider">
 <span>New to SnapCode?</span>
 </div>

 <Link to="/register" className="btn btn-secondary w-full">
 Create an account
 </Link>
 </div>
 </div>
 </div>
 );
}
