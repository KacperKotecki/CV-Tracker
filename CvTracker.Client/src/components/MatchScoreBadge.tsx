import './MatchScoreBadge.css'

interface MatchScoreBadgeProps {
  score: number | null
}

export default function MatchScoreBadge({ score }: MatchScoreBadgeProps) {
  if (score === null) return null
  const cls = score >= 70 ? 'match-badge--green'
            : score >= 40 ? 'match-badge--yellow'
                          : 'match-badge--red'
  return <span className={`match-badge ${cls}`}>{score}%</span>
}
