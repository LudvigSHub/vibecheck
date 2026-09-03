import { useState } from 'react'
import './App.css'
import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom'
import TestPage from './pages/TestPage';
import LandingPage from "./pages/LandingPage";
import HomePage from './pages/HomePage';
import Navbar from './components/Navbar';
import LoginForm from './components/auth/LoginForm';
import RegisterForm from './components/auth/RegisterForm';

function AppContent() {
  // null = ingen ruta öppen. Ligger här så knappen i navbaren når den.
  const [authView, setAuthView] = useState(null);
  const navigate = useNavigate();

  function handleAuthSuccess() {
    setAuthView(null);
    navigate('/home');
  }

  return (
    <>
      <Navbar onLoginClick={() => setAuthView('login')} />
      <Routes>
        {/* Route avser per page, element hämtas från pages där */}
        <Route path="/" element={<LandingPage />} />
        {/* Skyddet kopplas på här när ProtectedRoute-komponenten är klar. */}
        <Route path="/home" element={<HomePage />} />
        <Route path='/Test' element={<TestPage/>}/>
        {/* <Route path='/WordStash' element={<WordStash/>}/> */}
        {/* <Route path='/Account' element={<Account/>}/> */}
        {/* <Route path='/Quiz' element={<Quiz/>}/> */}

        {/* NEDAN AVSER SKYDD FÖR ATT INTE KUNNA NÅ ACCOUNT PAGE UTAN ATT VARA INLOGGAD. PROTECTEDROUTE ÄR EN EGEN KOMPONENT SOM ISF SKA IMPORTERAS */}
        {/* <Route path="/account" element={ <ProtectedRoute><AccountPage /></ProtectedRoute>}/> */}
      </Routes>

      {/* Ligger utanför Routes så de kan öppnas från vilken sida som helst */}
      {authView === 'login' && (
        <LoginForm
          onSuccess={handleAuthSuccess}
          onClose={() => setAuthView(null)}
          onSwitch={() => setAuthView('register')}
        />
      )}

      {authView === 'register' && (
        <RegisterForm
          onSuccess={handleAuthSuccess}
          onClose={() => setAuthView(null)}
          onSwitch={() => setAuthView('login')}
        />
      )}
    </>
  );
}

function App() {
  return (
    <Router>
      <AppContent />
    </Router>
  );
}

export default App
