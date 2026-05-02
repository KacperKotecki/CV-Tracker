import './App.css'
import { Outlet, Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import HomePage from './pages/HomePage'
import AddOfferPage from './pages/AddOfferPage'
import OfferDetailPage from './pages/OfferDetailPage'

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
                <Route path="/add" element={<AddOfferPage />} />
                <Route path="/offer/:id" element={<OfferDetailPage />} />
            </Route>
        </Routes>
    )
}


