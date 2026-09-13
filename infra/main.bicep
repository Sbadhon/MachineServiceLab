param location string = resourceGroup().location
param environmentName string = 'prod'

var suffix = uniqueString(subscription().id, resourceGroup().id)

var logAnalyticsName = 'log-msl-${environmentName}-${suffix}'
var applicationInsightsName = 'appi-msl-${environmentName}-${suffix}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location

  properties: {
    retentionInDays: 30

    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }

    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }

  sku: {
    name: 'PerGB2018'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'

  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output logAnalyticsName string = logAnalytics.name
output applicationInsightsName string = applicationInsights.name
output applicationInsightsResourceId string = applicationInsights.id