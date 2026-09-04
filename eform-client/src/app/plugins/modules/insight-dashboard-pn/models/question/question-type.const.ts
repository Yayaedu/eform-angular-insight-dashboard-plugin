// De 8 spørgsmålstyper tablet-appen (insight_app, Flutter) rent faktisk
// kan vise. Bevidst en delmængde af hvad den underliggende eForm-SDK
// understøtter (som også har smiley2-10/zipcode/info_text) — builderen
// tilbyder kun typer tabletten kan rendere, så man ikke kan oprette et
// spørgsmål der ikke kan vises.
//
// manualOptions: true betyder admin selv skal tilføje/redigere svar-
// muligheder (se OptionEditorComponent). For de øvrige typer genererer
// selve SDK'en automatisk de rigtige options, når spørgsmålet oprettes —
// se backend QuestionsService.
export interface QuestionTypeOption {
  value: string;
  label: string;
  manualOptions: boolean;
}

export const QUESTION_TYPES: QuestionTypeOption[] = [
  { value: 'smiley', label: 'Smiley', manualOptions: false },
  { value: 'buttons', label: 'Knapper', manualOptions: true },
  { value: 'list', label: 'Liste', manualOptions: true },
  { value: 'multi', label: 'Flervalg', manualOptions: true },
  { value: 'text', label: 'Fritekst', manualOptions: false },
  { value: 'number', label: 'Tal', manualOptions: false },
  { value: 'text_email', label: 'E-mail', manualOptions: false },
  { value: 'picture', label: 'Billede', manualOptions: false },
];

export function questionTypeLabel(value: string): string {
  return QUESTION_TYPES.find((t) => t.value === value)?.label ?? value;
}

export function questionTypeHasManualOptions(value: string): boolean {
  return QUESTION_TYPES.find((t) => t.value === value)?.manualOptions ?? false;
}
