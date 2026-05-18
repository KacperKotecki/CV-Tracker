import { NavLink } from 'react-router-dom'
import './Header.css'

export default function Header() {
  return (
    <header className="header">
      <NavLink to="/" className="header__logo">
        CV<span>Tracker</span>
      </NavLink>
      <nav className="header__nav">
        <NavLink to="/" end className="header__link">
          Oferty pracy
        </NavLink>
        <NavLink to="/dashboard" className={({ isActive }) => `header__link--outline${isActive ? ' active' : ''}`}>
          Dashboard
        </NavLink>
      </nav>
    </header>
  )
}
