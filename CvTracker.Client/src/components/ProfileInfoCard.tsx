import { useEffect, useRef, useState } from 'react'
import type { UserProfile, UpdateUserProfileRequest } from '../../models/UserProfile'
import './ProfileInfoCard.css'

interface ProfileInfoCardProps {
  profile: UserProfile | null
  isEditing: boolean
  onEditToggle: () => void
  onSave: (req: UpdateUserProfileRequest) => Promise<void>
  onCancel: () => void
  onAvatarUploaded: (avatarUrl: string) => void
  onResumeUploaded: (resumeFileName: string, resumeUrl: string) => void
}

const emptyDraft: UpdateUserProfileRequest = {
  firstName: null,
  lastName: null,
  location: null,
  linkedInUrl: null,
  gitHubUrl: null,
  websiteUrl: null,
}

export default function ProfileInfoCard({
  profile,
  isEditing,
  onEditToggle,
  onSave,
  onCancel,
  onAvatarUploaded,
  onResumeUploaded,
}: ProfileInfoCardProps) {
  const [draft, setDraft] = useState<UpdateUserProfileRequest>(emptyDraft)
  const [saving, setSaving] = useState(false)
  const [uploadingAvatar, setUploadingAvatar] = useState(false)
  const [uploadingResume, setUploadingResume] = useState(false)
  const avatarInputRef = useRef<HTMLInputElement>(null)
  const resumeInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (isEditing) {
      setDraft({
        firstName: profile?.firstName ?? null,
        lastName: profile?.lastName ?? null,
        location: profile?.location ?? null,
        linkedInUrl: profile?.linkedInUrl ?? null,
        gitHubUrl: profile?.gitHubUrl ?? null,
        websiteUrl: profile?.websiteUrl ?? null,
      })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isEditing])

  const handleChange = (field: keyof UpdateUserProfileRequest, value: string) => {
    setDraft(prev => ({ ...prev, [field]: value === '' ? null : value }))
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      await onSave(draft)
    } finally {
      setSaving(false)
    }
  }

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    setUploadingAvatar(true)
    try {
      const formData = new FormData()
      formData.append('file', file)
      const r = await fetch('/api/profile/avatar', { method: 'POST', body: formData })
      if (r.ok) {
        const data = await r.json() as { avatarUrl: string }
        onAvatarUploaded(data.avatarUrl)
      }
    } finally {
      setUploadingAvatar(false)
      if (avatarInputRef.current) avatarInputRef.current.value = ''
    }
  }

  const handleResumeChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    setUploadingResume(true)
    try {
      const formData = new FormData()
      formData.append('file', file)
      const r = await fetch('/api/profile/resume', { method: 'POST', body: formData })
      if (r.ok) {
        const data = await r.json() as { resumeFileName: string; resumeUrl: string }
        onResumeUploaded(data.resumeFileName, data.resumeUrl)
      }
    } finally {
      setUploadingResume(false)
      if (resumeInputRef.current) resumeInputRef.current.value = ''
    }
  }

  const fullName = [profile?.firstName, profile?.lastName].filter(Boolean).join(' ') || '—'

  if (!isEditing) {
    return (
      <div className="profile-card">
        <div className="profile-card__header">
          <h2 className="profile-card__title">Profil</h2>
          <button className="profile-card__edit-btn" onClick={onEditToggle} title="Edytuj profil">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
            </svg>
          </button>
        </div>
        <div className="profile-info">
          <div className="profile-info__top">
            {profile?.avatarUrl ? (
              <img
                className="profile-info__photo"
                src={profile.avatarUrl}
                alt="Avatar"
              />
            ) : (
              <div className="profile-info__photo profile-info__photo--placeholder">
                <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
                  <circle cx="12" cy="8" r="4" />
                  <path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" />
                </svg>
              </div>
            )}
            <div className="profile-info__details">
              <div className="profile-info__name">{fullName}</div>
              {profile?.location && (
                <div className="profile-info__location">
                  <span>📍</span>
                  <span>{profile.location}</span>
                </div>
              )}
            </div>
          </div>
          {(profile?.linkedInUrl || profile?.gitHubUrl || profile?.websiteUrl) && (
            <div className="profile-info__links">
              {profile.linkedInUrl && (
                <a className="profile-info__link" href={profile.linkedInUrl} target="_blank" rel="noreferrer">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z" />
                    <rect x="2" y="9" width="4" height="12" />
                    <circle cx="4" cy="4" r="2" />
                  </svg>
                  LinkedIn
                </a>
              )}
              {profile.gitHubUrl && (
                <a className="profile-info__link" href={profile.gitHubUrl} target="_blank" rel="noreferrer">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22" />
                  </svg>
                  GitHub
                </a>
              )}
              {profile.websiteUrl && (
                <a className="profile-info__link" href={profile.websiteUrl} target="_blank" rel="noreferrer">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <circle cx="12" cy="12" r="10" />
                    <line x1="2" y1="12" x2="22" y2="12" />
                    <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
                  </svg>
                  Website
                </a>
              )}
            </div>
          )}
          <div className={`profile-info__resume${!profile?.resumeFileName ? ' profile-info__resume--empty' : ''}`}>
            <span>📄</span>
            {profile?.resumeFileName && profile.resumeUrl ? (
              <a href={profile.resumeUrl} target="_blank" rel="noreferrer" className="profile-info__resume-link">
                {profile.resumeFileName}
              </a>
            ) : (
              <span>Brak CV</span>
            )}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="profile-card">
      <div className="profile-card__header">
        <h2 className="profile-card__title">Edytuj profil</h2>
      </div>
      <div className="profile-info__form">
        <div className="profile-info__row">
          <div className="profile-info__field">
            <label className="profile-info__label">Imię</label>
            <input
              className="profile-info__input"
              type="text"
              value={draft.firstName ?? ''}
              onChange={e => handleChange('firstName', e.target.value)}
              placeholder="Jan"
            />
          </div>
          <div className="profile-info__field">
            <label className="profile-info__label">Nazwisko</label>
            <input
              className="profile-info__input"
              type="text"
              value={draft.lastName ?? ''}
              onChange={e => handleChange('lastName', e.target.value)}
              placeholder="Kowalski"
            />
          </div>
        </div>
        <div className="profile-info__row profile-info__row--full">
          <div className="profile-info__field">
            <label className="profile-info__label">Lokalizacja</label>
            <input
              className="profile-info__input"
              type="text"
              value={draft.location ?? ''}
              onChange={e => handleChange('location', e.target.value)}
              placeholder="Warszawa, PL"
            />
          </div>
        </div>
        <div className="profile-info__row">
          <div className="profile-info__field">
            <label className="profile-info__label">LinkedIn URL</label>
            <input
              className="profile-info__input"
              type="url"
              value={draft.linkedInUrl ?? ''}
              onChange={e => handleChange('linkedInUrl', e.target.value)}
              placeholder="https://linkedin.com/in/..."
            />
          </div>
          <div className="profile-info__field">
            <label className="profile-info__label">GitHub URL</label>
            <input
              className="profile-info__input"
              type="url"
              value={draft.gitHubUrl ?? ''}
              onChange={e => handleChange('gitHubUrl', e.target.value)}
              placeholder="https://github.com/..."
            />
          </div>
        </div>
        <div className="profile-info__row profile-info__row--full">
          <div className="profile-info__field">
            <label className="profile-info__label">Strona www</label>
            <input
              className="profile-info__input"
              type="url"
              value={draft.websiteUrl ?? ''}
              onChange={e => handleChange('websiteUrl', e.target.value)}
              placeholder="https://..."
            />
          </div>
        </div>
        <div className="profile-info__row">
          <div className="profile-info__field">
            <label className="profile-info__label">
              Zdjęcie profilowe {uploadingAvatar && <span className="profile-info__uploading">Wysyłanie…</span>}
            </label>
            <input
              ref={avatarInputRef}
              className="profile-info__input profile-info__input--file"
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleAvatarChange}
              disabled={uploadingAvatar}
            />
          </div>
          <div className="profile-info__field">
            <label className="profile-info__label">
              CV (PDF/DOC) {uploadingResume && <span className="profile-info__uploading">Wysyłanie…</span>}
            </label>
            <input
              ref={resumeInputRef}
              className="profile-info__input profile-info__input--file"
              type="file"
              accept=".pdf,.doc,.docx"
              onChange={handleResumeChange}
              disabled={uploadingResume}
            />
          </div>
        </div>
        <div className="profile-info__actions">
          <button
            className="profile-info__btn profile-info__btn--save"
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? 'Zapisywanie…' : 'Zapisz'}
          </button>
          <button
            className="profile-info__btn profile-info__btn--cancel"
            onClick={onCancel}
            disabled={saving}
          >
            Anuluj
          </button>
        </div>
      </div>
    </div>
  )
}
