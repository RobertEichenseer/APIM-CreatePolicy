# Azure API Management (APIM) - Azure CLI policy support

## tl;dr

[CreateEnv.azcli](/src/CreateEnv/CreateEnv.azcli) is a PowerShell script to create a demo environment to showcase the creation / list of Azure API Management policies using the Azure CLI and Azure REST API calls as the CLI command `az apim`does not yet support policy commands. 
![Demo Environment](/img/DemoScenario.png)

There are [payload examples](/src/CreateEnv/Policies/) which have to be posted to the Azure REST Api to create policies. The [demo script](/src/CreateEnv/CreateEnv.azcli) shows in Step 13 the REST Api calls. 

## Individual steps

The [PowerShell script](/src/CreateEnv/CreateEnv.azcli) includes the following steps: 

- **Step 1:**  
  Login to Azure, retrieve existing subscriptions according to the logged in user and select the default subscription.
- **Step 2:**
  Defines a project unifier to allow multiple execution of the script in the same environment
- **Step 3:**  
  Creates a resource group where all Azure services (APIM + Azure Web App) will be created.
- **Step 4:**
  Creates an Azure API Management instance (SKU = Basic, Capacity = 1). The provisioning of the APIM instance takes some time. The status of the provisioning can be checked with:

  ```Powershell
    az apim show `
        --name $apimName `
        --resource-group $resourceGroup `
        --query "provisioningState" `
        --output tsv
    ```

    where the provisioningState needs to be "Succeeded"
- **Step 5 and Step 6:**  
  While the API Management instance will be provisioned it is possible to create an App Service Plan to host an Azure Web App which provides two RESTful API method.
- **Step 7:**  
  [Publishes a .NET Core application](src/APIM.Service/) and deploys the application to the in Step 6 created Azure Web App instance. The application provides a RESTful API with the methods `WeatherForecast()` and `WeatherForecastCity(string city)`
  ![RESTful API](/img/RESTfulInterface.png)

  *CreateWeatherForecast()*:  
  Returns an array with date, temperatureC, temperatureF and descripton AND in the response Header `APIM-Source-System` with the value `WeatherForecastController`
  ```powershell
  curl --verbose http://localhost:<REPLACE WITH LOCAL PORTNUMBER>/Weatherforecast
  GET /Weatherforecast HTTP/1.1
  User-Agent: curl/7.83.1
  ...
  HTTP/1.1 200 OK
  APIM-Source-System: WeatherForecastController
  [{"date":"2023-02-13","temperatureC":33,"temperatureF":91,"summary":"Hot"},
  {"date":"2023-02-14","temperatureC":-10,"temperatureF":15,"summary":"Hot"},
  {"date":"2023-02-15","temperatureC":11,"temperatureF":51,"summary":"Bracing"},
  {"date":"2023-02-16","temperatureC":54,"temperatureF":129,"summary":"Hot"},
  {"date":"2023-02-17","temperatureC":2,"temperatureF":35,"summary":"Warm"}]
  ```

  *CreateWeatherForecastCity(string city)*:  
  Returns an array with the name of the provided city, region AND in the response Header `APIM-Source-System` with the value `WeatherForecastController`

  ```Powershell
  curl --verbose http://localhost:<REPLACE WITH LOCAL PORTNUMBER>/WeatherforecastCity?City=Munich
  GET /WeatherforecastCity?City=Munich HTTP/1.1
  HTTP/1.1 200 OK
  APIM-Source-System: WeatherForecastController
  [{"city":"Munich","region":"Europe"},
  {"city":"Other City","region":"Other Region"}]
  ```

  After Azure API Management is fully configured a call to `/WeatherForecast` will be routed to `/WeatherForecastCity?City=Munich` AND the Header `APIM-Source-System` will be removed. 

- **Step 8:**  
  Imports the RESTful API provided by the .NET Core app and hosted by the Azure Web App as an API into Azure API Management. To import the RESTful API the OpenApi format is used. OpenApi information are provided by the .NET Core App (/swagger/v1/swagger.json). 

- **Step 9:**
  Within Azure API Management a the product is created `$apimProductNAme` & `$apimProductId`

- **Step 10:**
  The API imported in Step 8 will be added to the product created in Step 9

- **Step 11:**
  So far no policies have been applied to the API therefore both calls will return the values mentioned under Step 7

- **Step 12:**
  Using the Azure REST API interface directly instead of Azure CLI functionality (which does not yet exist) policies for global (all APIs) scope, product scope, API scope and API operation scope will be created. The following [policy definitions](Policies) will be put to the specific Azure REST API interface.

- **Step 13:**
  Checks if the policies are working.
  The following response is expected:

  ```powershell
  curl --verbose $apimServiceUrl
  GET /weather/WeatherForecast HTTP/1.1
  Host: apimsample201.azure-api.net
  HTTP/1.1 200 OK
  APIM-ProductScopePolicy: Added on Product Level
  APIM-AllApiScopePolicy: Added on global level (all apis)
  APIM-APIScopePolicy: Added on Api level
  [{"city":"Munich","region":"Europe"},
  {"city":"Other City","region":"Other Region"}]
  ```
 
  - Header `APIM-AllApiScopePolicy`is added by a policy on global (all APIs) scope
  - Header `APIM-ProductScopePolicy` is added by a policy on product scope
  - Header `APIM-APIScopePolicy` is added by a policy on product scope
  - The Header `APIM-Source-System` delivered by the .NET core application is removed by the policy on global (all APIs) scope.
  - The call to `/WeatherForecast` is routed to `/WeatherForecastCity?City=Munich` by the policy on API operational scope
