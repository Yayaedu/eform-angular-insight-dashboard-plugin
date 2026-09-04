import { OptionModel } from './option.model';

export class QuestionModel {
  id: number;
  questionSetId: number;
  text: string;
  questionType: string;
  questionIndex: number;
  options: OptionModel[] = [];
}
