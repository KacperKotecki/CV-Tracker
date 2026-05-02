import { useNavigate } from "react-router-dom";
import type { JobOffer } from "../../models/JobOffer";
import './JobOfferCard.css'

interface JobOfferCardProps {
  jobOffer: JobOffer
}



export default function JobOfferCard({ jobOffer }: JobOfferCardProps) {
  const navigate = useNavigate()

  const goToEditOffertById = async (id : number) => {
  navigate(`./edit/${id}`)
}


  return (
    <div className="job-offer-card">
      <div className="job-offer-card__header">
        <h2>{jobOffer.position}</h2>
        <span className="job-offer-card__salary">{jobOffer.salary} PLN</span>
      </div>

      <div className="job-offer-card__tags">
        <span className="tag">{jobOffer.contractType}</span>
        <span className="tag">{jobOffer.workMode}</span>
        <span className="tag">{jobOffer.workLoad}</span>
      </div>

      <p className="job-offer-card__company">
        {jobOffer.company?.companyName} · {jobOffer.company?.companyAddress}
      </p>

      <div className="job-offer-card__body">
          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Umiejętności</span>
            <span>{jobOffer.skills}</span>
          </div>
        
          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Wymagania</span>
            <span>{jobOffer.ourRequirements}</span>
          </div>
        
          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Co oferujemy</span>
            <span>{jobOffer.whatWeOffer}</span>
          </div>

          <div className="job-offer-card__row">
            <span className="job-offer-card__label">Benefity</span>
            <span>{jobOffer.benefits}</span>
          </div>

        <button className='btn' onClick={() => goToEditOffertById(jobOffer.id)}>Edytuj oferte</button>
      </div>
    </div>
  );
}
