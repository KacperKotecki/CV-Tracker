import { useEffect, useRef, useState } from 'react'
import type { UserProfile, UpdateUserProfileRequest } from '../../models/UserProfile'
import type { UserTechnology, UpdateUserTechnologiesRequest } from '../../models/UserSkill'
import type { TechnologyCategory } from '../../models/Technology'
import type { SkillLevel } from '../../models/SkillLevel'
import ProfileInfoCard from '../components/ProfileInfoCard'
import SkillsCard from '../components/SkillsCard'
import './ProfilePage.css'

export default function ProfilePage() {
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [categories, setCategories] = useState<TechnologyCategory[]>([])
  const [isEditingProfile, setIsEditingProfile] = useState(false)
  const [isEditingSkills, setIsEditingSkills] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const skillsRequestCounter = useRef(0)

  useEffect(() => {
    fetch('/api/profile')
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`)
        return r.json() as Promise<UserProfile>
      })
      .then(data => {
        setProfile(data)
        setLoading(false)
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Błąd pobierania profilu')
        setLoading(false)
      })
  }, [])

  useEffect(() => {
    fetch('/api/technologies')
      .then(r => r.json())
      .then((data: TechnologyCategory[]) => setCategories(data))
  }, [])

  const handleSaveProfile = async (req: UpdateUserProfileRequest): Promise<void> => {
    const r = await fetch('/api/profile', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(req),
    })
    if (r.ok) {
      const updated = await r.json() as UserProfile
      setProfile(updated)
      setIsEditingProfile(false)
    }
  }

  const handleAvatarUploaded = (avatarUrl: string) => {
    setProfile(prev => prev ? { ...prev, avatarUrl } : prev)
  }

  const handleResumeUploaded = (resumeFileName: string, resumeUrl: string) => {
    setProfile(prev => prev ? { ...prev, resumeFileName, resumeUrl } : prev)
  }

  const buildSkillsRequest = (updatedSkills: UserTechnology[]): UpdateUserTechnologiesRequest => ({
    technologies: updatedSkills.map(s => ({
      technologyId: s.technologyId,
      level: s.level,
    })),
  })

  const handleSkillToggle = async (technologyId: number): Promise<void> => {
    if (!profile) return
    const exists = profile.skills.find(s => s.technologyId === technologyId)
    const updatedSkills: UserTechnology[] = exists
      ? profile.skills.filter(s => s.technologyId !== technologyId)
      : [...profile.skills, { id: 0, technologyId, technologyName: '', category: '', level: 'Mid' as SkillLevel }]

    setProfile(prev => prev ? { ...prev, skills: updatedSkills } : prev)

    const reqId = ++skillsRequestCounter.current
    const r = await fetch('/api/profile/skills', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(buildSkillsRequest(updatedSkills)),
    })
    if (r.ok && reqId === skillsRequestCounter.current) {
      const saved = await r.json() as UserTechnology[]
      setProfile(prev => prev ? { ...prev, skills: saved } : prev)
    }
  }

  const handleLevelChange = async (
    technologyId: number,
    level: SkillLevel,
  ): Promise<void> => {
    if (!profile) return
    const updatedSkills = profile.skills.map(s =>
      s.technologyId === technologyId ? { ...s, level } : s,
    )

    setProfile(prev => prev ? { ...prev, skills: updatedSkills } : prev)

    const reqId = ++skillsRequestCounter.current
    const r = await fetch('/api/profile/skills', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(buildSkillsRequest(updatedSkills)),
    })
    if (r.ok && reqId === skillsRequestCounter.current) {
      const saved = await r.json() as UserTechnology[]
      setProfile(prev => prev ? { ...prev, skills: saved } : prev)
    }
  }

  if (loading) {
    return (
      <div className="profile-page">
        <div className="profile-page__loading">Ładowanie profilu…</div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="profile-page">
        <div className="profile-page__error">Błąd: {error}</div>
      </div>
    )
  }

  return (
    <div className="profile-page">
      <div className="profile-page__content">
        <ProfileInfoCard
          profile={profile}
          isEditing={isEditingProfile}
          onEditToggle={() => setIsEditingProfile(v => !v)}
          onSave={handleSaveProfile}
          onCancel={() => setIsEditingProfile(false)}
          onAvatarUploaded={handleAvatarUploaded}
          onResumeUploaded={handleResumeUploaded}
        />
        <SkillsCard
          skills={profile?.skills ?? []}
          categories={categories}
          isEditing={isEditingSkills}
          onEditToggle={() => setIsEditingSkills(v => !v)}
          onSkillToggle={handleSkillToggle}
          onLevelChange={handleLevelChange}
          onCancel={() => setIsEditingSkills(false)}
        />
      </div>
    </div>
  )
}

