import { useNavigate } from "react-router-dom";
import type { JobOffer } from "../../models/JobOffer";
import './JobOfferCard.css'

interface JobOfferCardProps {
  jobOffer: JobOffer
}

function salaryRange(jobOffer: JobOffer): string | null {
  const { salaryMin, salaryMax } = jobOffer
  if (salaryMin != null && salaryMax != null)
    return `${salaryMin.toLocaleString('pl-PL')} – ${salaryMax.toLocaleString('pl-PL')} PLN`
  if (salaryMin != null) return `od ${salaryMin.toLocaleString('pl-PL')} PLN`
  if (salaryMax != null) return `do ${salaryMax.toLocaleString('pl-PL')} PLN`
  return null
}

export default function JobOfferCard({ jobOffer }: JobOfferCardProps) {
  const navigate = useNavigate()

  const salary = salaryRange(jobOffer)

  return (
    <div className="job-offer-card" onClick={() => navigate(`/offer/${jobOffer.id}`)}>
      <div className="job-offer-card__header">
        {jobOffer.sourceUrl
          ? <h2><a href={jobOffer.sourceUrl} target="_blank" rel="noopener noreferrer" onClick={e => e.stopPropagation()}>{jobOffer.position}</a></h2>
          : <h2>{jobOffer.position}</h2>}
        {salary && <span className="job-offer-card__salary">{salary}</span>}
      </div>

      <div className="job-offer-card__tags">
        <span className="tag">{jobOffer.contractType}</span>
        <span className="tag">{jobOffer.workMode}</span>
        <span className="tag">{jobOffer.workLoad}</span>
      </div>

      <p className="job-offer-card__company">
        {jobOffer.companyName ?? '—'}{jobOffer.location ? ` · ${jobOffer.location}` : ''}
      </p>

      <div className="job-offer-card__body">
          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Umiejętności</span>
            <span>{jobOffer.skills}</span>
          </div>

          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Status</span>
            <span>{jobOffer.status}</span>
          </div>

        <button className='btn' onClick={e => { e.stopPropagation(); navigate(`/edit/${jobOffer.id}`) }}>Edytuj ofertę</button>
      </div>
    </div>
  );
}
