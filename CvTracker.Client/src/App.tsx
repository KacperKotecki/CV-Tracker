import './App.css'
import { Outlet, Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import OffersPage from './pages/OffersPage'
import Dashboard from './pages/Dashboard'

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
      <Route path="/" element={<OffersPage />} />
      <Route element={<Layout />}>
        <Route path="/dashboard" element={<Dashboard />} />
      </Route>
    </Routes>
  )
}

