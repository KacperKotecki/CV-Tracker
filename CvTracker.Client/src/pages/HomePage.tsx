import { useEffect, useState } from "react"
import JobOfferCard from "../components/JobOfferCard"
import type { JobOffer } from "../../models/JobOffer"


export default function OfferHomePage() {
    const [offers, setOffers] = useState<JobOffer[]>([])
    
    useEffect(() => {
        fetch('/api/jobapplications').then(r => r.json()).then(setOffers)
    }, [])
    
    return <div> {offers.map(offer => <JobOfferCard key={offer.id} jobOffer={offer} />)}</div>
}