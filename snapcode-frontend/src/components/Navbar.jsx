import { useAuth } from '../contexts/AuthContext';
import { useNavigate, useLocation } from 'react-router-dom';
import './Navbar.css';

export default function Navbar() {
 const { user, logout } = useAuth();
 const navigate = useNavigate();
 const location = useLocation();

 async function handleLogout() {
 await logout();
 navigate('/login');
 }

 const isEditor = location.pathname.startsWith('/editor');

 return (
 <nav className={`navbar ${isEditor ? 'navbar-compact' : ''}`}>
 <div className="navbar-inner">
 {/* Brand */}
 <button className="navbar-brand" onClick={() => navigate('/dashboard')}>
 <span className="navbar-logo-icon"></span>
 <span className="navbar-logo-text">SnapCode</span>
 </button>

 {/* Right side */}
 <div className="navbar-actions">
 {user && (
 <>
 {isEditor && (
 <button
 className="btn btn-ghost btn-sm"
 onClick={() => navigate('/dashboard')}
 >
 ← Projects
 </button>
 )}

 <div className="navbar-user">
 <div className="navbar-avatar">
 {user.displayName?.[0]?.toUpperCase() || user.email?.[0]?.toUpperCase() || '?'}
 </div>
 <span className="navbar-username">{user.displayName || user.email}</span>
 </div>

 <button className="btn btn-ghost btn-sm" onClick={handleLogout}>
 Sign Out
 </button>
 </>
 )}
 </div>
 </div>
 </nav>
 );
}
