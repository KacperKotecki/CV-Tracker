export const contractTypeOptions = ['UoP', 'B2B', 'MandateContract', 'SpecificWorkContract', 'Internship', 'Apprenticeship'] as const
export type ContractType = typeof contractTypeOptions[number]
