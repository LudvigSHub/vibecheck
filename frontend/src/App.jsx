import './App.css'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import TestPage from './pages/TestPage';
import LandingPage from "./pages/LandingPage";
import Navbar from './components/Navbar';
import AuthPreview from './pages/AuthPreview';

function App() {

  return (
    <Router>
      <Navbar />
      <Routes>
        {/* Route avser per page, element hämtas från pages där */}
        <Route path="/" element={<LandingPage />} />
        <Route path='/Test' element={<TestPage/>}/>
        {/* Tillfällig, bara för att titta på inloggning och registrering */}
        <Route path='/auth-preview' element={<AuthPreview/>}/>
        {/* <Route path='/WordStash' element={<WordStash/>}/> */}
        {/* <Route path='/Account' element={<Account/>}/> */}
        {/* <Route path='/Quiz' element={<Quiz/>}/> */}

        {/* NEDAN AVSER SKYDD FÖR ATT INTE KUNNA NÅ ACCOUNT PAGE UTAN ATT VARA INLOGGAD. PROTECTEDROUTE ÄR EN EGEN KOMPONENT SOM ISF SKA IMPORTERAS */}
        {/* <Route path="/account" element={ <ProtectedRoute><AccountPage /></ProtectedRoute>}/> */}
      </Routes>
    </Router>
  );
}

export default App
