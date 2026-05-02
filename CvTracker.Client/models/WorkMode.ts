export const workModeOptions = ['OnSite', 'Remote', 'Hybrid'] as const
export type WorkMode = typeof workModeOptions[number]
