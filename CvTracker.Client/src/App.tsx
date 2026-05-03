import './App.css'
import { Outlet, Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import HomePage from './pages/HomePage'
import OfferDetailPage from './pages/OfferDetailPage'
import AddEditOfferPage from './pages/AddEditOfferPage'
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
            <Route element={<Layout />}>
                <Route path="/" element={<HomePage />} />
                <Route path="/add" element={<AddEditOfferPage />} />
                <Route path="/edit/:id" element={<AddEditOfferPage />} />
                <Route path="/offer/:id" element={<OfferDetailPage />} />
                <Route path="/dashboard" element={<Dashboard />} />
            </Route>
        </Routes>
    )
}


