import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import LoginForm from '../components/auth/LoginForm';
import RegisterForm from '../components/auth/RegisterForm';

// Tillfällig testsida. Tas bort när formulären ligger på landningssidan.
function AuthPreview() {
  const { user, isAuthenticated, authLoading, logout } = useAuth();
  const [visa, setVisa] = useState(null);

  if (authLoading) {
    return <p>Laddar...</p>;
  }

  if (isAuthenticated) {
    return (
      <div style={{ padding: 24 }}>
        <h1>Inloggad som {user.userName}</h1>
        <p>Roller: {user.roles?.join(', ') || 'inga'}</p>
        <button onClick={logout}>Logga ut</button>
      </div>
    );
  }

  return (
    <div style={{ padding: 24, display: 'flex', gap: 12 }}>
      <button onClick={() => setVisa('login')}>Logga in</button>
      <button onClick={() => setVisa('register')}>Skapa konto</button>

      {visa === 'login' && (
        <LoginForm
          onClose={() => setVisa(null)}
          onSwitch={() => setVisa('register')}
        />
      )}

      {visa === 'register' && (
        <RegisterForm
          onClose={() => setVisa(null)}
          onSwitch={() => setVisa('login')}
        />
      )}
    </div>
  );
}

export default AuthPreview;
