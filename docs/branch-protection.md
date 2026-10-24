# Правила защиты ветки main

Ветка main защищена в настройках репозитория (Settings > Branches > Branch protection rules). Прямой push в нее запрещен, изменения попадают туда только через Pull Request.

| Правило | Значение |
|---|---|
| Require a pull request before merging | Включено |
| Required approvals | 1 |
| Dismiss stale pull request approvals when new commits are pushed | Включено |
| Require review from Code Owners | Включено (файлы в sql/ согласует администратор БД) |
| Require status checks to pass before merging | Включено, обязательная проверка build из .github/workflows/ci.yml |
| Require branches to be up to date before merging | Включено |
| Require conversation resolution before merging | Включено |
| Require linear history | Включено |
| Allow force pushes | Запрещено |
| Allow deletions | Запрещено |
| Do not allow bypassing the above settings | Включено, правила действуют и для администраторов |

Одного одобрения достаточно, потому что над сервисом парковки работает небольшая команда. Риск снижает обязательное согласование владельцем кода для SQL-скриптов, так как ошибка в миграции опаснее ошибки в коде контроллера. Название коммита должно начинаться с номера задачи в квадратных скобках, например [CAMPUS-109], чтобы изменение можно было связать с задачей в Jira.
