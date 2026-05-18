import type { ApplicationStatus } from '../../models/ApplicationStatus'
import type { JobOffer } from '../../models/JobOffer'

export function salaryRange(offer: JobOffer): string {
  const { salaryMin, salaryMax } = offer
  if (salaryMin != null && salaryMax != null)
    return `${salaryMin.toLocaleString('pl-PL')} – ${salaryMax.toLocaleString('pl-PL')} PLN`
  if (salaryMin != null) return `od ${salaryMin.toLocaleString('pl-PL')} PLN`
  if (salaryMax != null) return `do ${salaryMax.toLocaleString('pl-PL')} PLN`
  return '—'
}

export function formatRelativeDate(value: string | null | undefined): string {
  if (!value) return '—'
  const date = new Date(value)
  if (isNaN(date.getTime())) return '—'
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  if (days === 0) return 'dziś'
  if (days === 1) return 'wczoraj'
  if (days < 7) return `${days} dni temu`
  if (days < 30) return `${Math.floor(days / 7)} tyg. temu`
  if (days < 365) return `${Math.floor(days / 30)} mies. temu`
  return `${Math.floor(days / 365)} lat temu`
}

export function statusColor(status: ApplicationStatus): string {
  switch (status) {
    case 'Draft': return '#9ca3af'
    case 'Applied': return '#3b82f6'
    case 'HRScreening': return '#f59e0b'
    case 'TechnicalInterview': return '#8b5cf6'
    case 'LiveCodingOrAssignment': return '#ec4899'
    case 'AwaitingFeedback': return '#06b6d4'
    case 'Rejected': return '#ef4444'
    case 'Accepted': return '#10b981'
    case 'Ghosted': return '#6b7280'
    default: return '#9ca3af'
  }
}
