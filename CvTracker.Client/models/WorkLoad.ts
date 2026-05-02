export const workLoadOptions = ['FullTime', 'PartTime'] as const
export type WorkLoad = typeof workLoadOptions[number]
