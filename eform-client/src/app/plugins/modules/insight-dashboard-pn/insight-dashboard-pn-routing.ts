import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {InsightDashboardPnLayoutComponent} from './layouts';
import {AdminGuard, AuthGuard} from '../../../common/guards';
import {
  AnswerPageComponent,
  DashboardEditComponent,
  DashboardsPageComponent,
  DashboardViewComponent,
  InsightDashboardSettingsComponent,
  QuestionSetEditorPageComponent,
  QuestionSetsPageComponent,
  SurveyConfigurationsPageComponent,
} from './components';

export const routes: Routes = [
  {
    path: '',
    component: InsightDashboardPnLayoutComponent,
    // canActivate: [PermissionGuard],
    // data: {requiredPermission: InsightDashboardPnClaims.accessInsightDashboardPlugin},
    children: [
      {
        path: 'dashboards',
        canActivate: [AuthGuard],
        component: DashboardsPageComponent,
      },
      {
        path: 'dashboard/:dashboardId',
        canActivate: [AuthGuard],
        component: DashboardViewComponent,
      },
      {
        path: 'dashboard/edit/:dashboardId',
        canActivate: [AuthGuard],
        component: DashboardEditComponent,
      },
      {
        path: 'surveys-configs',
        canActivate: [AuthGuard],
        component: SurveyConfigurationsPageComponent,
      },
      {
        path: 'question-sets',
        canActivate: [AuthGuard],
        component: QuestionSetsPageComponent,
      },
      {
        path: 'question-sets/:id',
        canActivate: [AuthGuard],
        component: QuestionSetEditorPageComponent,
      },
      {
        path: 'answers',
        canActivate: [AdminGuard],
        component: AnswerPageComponent,
      },
      {
        path: 'settings',
        canActivate: [AdminGuard],
        component: InsightDashboardSettingsComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InsightDashboardPnRouting {
}
