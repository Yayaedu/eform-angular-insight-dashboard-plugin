import { QuestionModel } from './question.model';

export class QuestionSetModel {
  id: number;
  name: string;
  questions: QuestionModel[] = [];
}
