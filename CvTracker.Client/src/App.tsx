import './App.css'
import { Outlet, Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import OffersPage from './pages/OffersPage'
import Dashboard from './pages/Dashboard'
import ProfilePage from './pages/ProfilePage'
import ScrapingPage from './pages/ScrapingPage'

function Layout() {
  return (
    <>
      <Header />
      <main>
        <Outlet />
      </main>
    </>
  )
}

// App.tsx — TYLKO trasy
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<OffersPage />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/scraping/:id" element={<ScrapingPage />} />
      </Route>
    </Routes>
  )
}

