const guidPattern =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

const emptyGuid = '00000000-0000-0000-0000-000000000000'

export function isGuid(value: string): boolean {
  const normalized = value.trim().toLowerCase()
  return normalized !== emptyGuid && guidPattern.test(normalized)
}
