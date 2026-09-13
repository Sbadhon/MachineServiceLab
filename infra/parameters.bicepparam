using './main.bicep'

param location = 'centralus'
param environmentName = 'prod'

param sqlEntraAdminName =
  readEnvironmentVariable('AZURE_SQL_ADMIN_NAME')

param sqlEntraAdminSid =
  readEnvironmentVariable('AZURE_CLIENT_ID')