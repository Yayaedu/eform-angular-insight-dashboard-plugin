import {NgModule} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {TranslateModule} from '@ngx-translate/core';
import {EformSharedModule} from 'src/app/common/modules/eform-shared/eform-shared.module';
import {InsightDashboardPnLayoutComponent} from './layouts';
import {InsightDashboardPnRouting} from './insight-dashboard-pn-routing';
import {
  InsightDashboardPnAnswersService,
  InsightDashboardPnDashboardDictionariesService,
  InsightDashboardPnDashboardItemsService,
  InsightDashboardPnDashboardsService,
  InsightDashboardPnDevicePairingService,
  InsightDashboardPnOptionsService,
  InsightDashboardPnQuestionSetsService,
  InsightDashboardPnQuestionsService,
  InsightDashboardPnSettingsService,
  InsightDashboardPnSurveyConfigsService,
} from './services';
import {
  AnswerDeleteModalComponent,
  AnswerPageComponent,
  AnswerValuesTableComponent,
  OptionEditorComponent,
  QuestionDeleteComponent,
  QuestionEditComponent,
  QuestionSetDeleteComponent,
  QuestionSetEditComponent,
  QuestionSetEditorPageComponent,
  QuestionSetsPageComponent,
  DashboardBlockViewComponent,
  DashboardChartDataEditComponent,
  DashboardChartDataViewComponent,
  DashboardChartEditComponent,
  DashboardChartViewComponent,
  DashboardCopyComponent,
  DashboardDeleteComponent,
  DashboardEditComponent,
  DashboardEditHeaderComponent,
  DashboardInterviewsEditComponent,
  DashboardInterviewsViewComponent,
  DashboardItemEditComponent,
  DashboardNewComponent,
  DashboardsPageComponent,
  DashboardViewComponent,
  InsightDashboardSettingsComponent,
  SurveyConfigurationDeleteComponent,
  SurveyConfigurationEditComponent,
  SurveyConfigurationNewComponent,
  SurveyConfigurationsPageComponent,
  SurveyConfigurationStatusComponent,
  DevicePairingPageComponent,
  DevicePairingCodeModalComponent,
} from './components';
import {DragulaModule} from 'ng2-dragula';
import {NgxChartsModule} from '@swimlane/ngx-charts';
import {MatButtonModule} from '@angular/material/button';
import {MtxGridModule} from '@ng-matero/extensions/grid';
import {MatInputModule} from '@angular/material/input';
import {MatDialogModule} from '@angular/material/dialog';
import {MtxSelectModule} from '@ng-matero/extensions/select';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatCardModule} from '@angular/material/card';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {StoreModule} from '@ngrx/store';
import * as dashboardsReducer from './state/dashboards/dashboards.reducer';
import * as surveysReducer from './state/surveys/surveys.reducer';
import {MatMenu, MatMenuItem, MatMenuTrigger} from "@angular/material/menu";

@NgModule({
  imports: [
    InsightDashboardPnRouting,
    TranslateModule,
    FormsModule,
    EformSharedModule,
    DragulaModule,
    NgxChartsModule,
    MatButtonModule,
    MtxGridModule,
    MatInputModule,
    MatDialogModule,
    MtxSelectModule,
    MatCheckboxModule,
    MatCardModule,
    MatIconModule,
    MatTooltipModule,
    MatExpansionModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    StoreModule.forFeature('insightDashboardPn', {
      dashboardsState: dashboardsReducer.reducer,
      surveysState: surveysReducer.reducer,
    }),
    MatMenu,
    MatMenuTrigger,
    MatMenuItem,
  ],
  declarations: [
    InsightDashboardPnLayoutComponent,
    InsightDashboardSettingsComponent,
    SurveyConfigurationsPageComponent,
    DashboardsPageComponent,
    DashboardNewComponent,
    SurveyConfigurationNewComponent,
    SurveyConfigurationEditComponent,
    DashboardEditComponent,
    DashboardViewComponent,
    SurveyConfigurationStatusComponent,
    SurveyConfigurationDeleteComponent,
    DashboardDeleteComponent,
    DashboardCopyComponent,
    DashboardItemEditComponent,
    DashboardChartEditComponent,
    DashboardChartViewComponent,
    DashboardBlockViewComponent,
    DashboardEditHeaderComponent,
    DashboardInterviewsEditComponent,
    DashboardChartDataEditComponent,
    DashboardChartDataViewComponent,
    // DashboardInterviewsViewComponent,
    AnswerPageComponent,
    AnswerValuesTableComponent,
    AnswerDeleteModalComponent,
    DashboardInterviewsViewComponent,
    QuestionSetsPageComponent,
    QuestionSetEditComponent,
    QuestionSetDeleteComponent,
    QuestionSetEditorPageComponent,
    QuestionEditComponent,
    QuestionDeleteComponent,
    OptionEditorComponent,
    DevicePairingPageComponent,
    DevicePairingCodeModalComponent,
  ],
  providers: [
    InsightDashboardPnSettingsService,
    InsightDashboardPnDashboardsService,
    InsightDashboardPnSurveyConfigsService,
    InsightDashboardPnDashboardDictionariesService,
    InsightDashboardPnDashboardItemsService,
    InsightDashboardPnAnswersService,
    InsightDashboardPnQuestionSetsService,
    InsightDashboardPnQuestionsService,
    InsightDashboardPnOptionsService,
    InsightDashboardPnDevicePairingService,
  ],
})
export class InsightDashboardPnModule {
}
