$resourceGroupName = "speaking-cloud-native"
$resourceGroupName2 = "speaking-cloud-native2"
$subscriptionName = "Subscription 19"

# Login
az login
az account set --subscription $subscriptionName

# Create resource group
az group create -n $resourceGroupName -l WestEurope 
az group create -n $resourceGroupName2 -l WestEurope 

## PART 1

# Create app service plan
az appservice plan create -n "gallery-plan" -g $resourceGroupName -l WestEurope --sku S1 

# Create app service
az webapp create -g $resourceGroupName -p "gallery-plan" -n "gallery-app"
az webapp config appsettings set -g $resourceGroupName -n "gallery-app" --settings AppSettings:PhotosDirectory=D:\home\site\wwwroot\wwwroot\Photos

# Create storage account
az storage account create -g $resourceGroupName -n "galleryazmstorage" -l WestEurope --sku Standard_LRS 

# TODO: deploy the apps from Visual Studio
# TODO: specify correct connection string in the portal


## PART 2

# Create SQL Server and allow Azure services through firewall
az sql server create -g $resourceGroupName -l WestEurope -n "gallery-sql" -u AzureMigration -p 8jLmj7Z7LKetGdrf
az sql server firewall-rule create -g $resourceGroupName -s "gallery-sql" -n "azureaccessrule" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# TODO: migrate the database from SQL Server Management Studio


## PART 3

# TODO: configure DevOps pipeline


## PART 4

# Create Container Registry, get password and login in Docker client
az acr create -g $resourceGroupName2 -l WestEurope -n "galleryazmregistry" --admin-enabled true --sku Standard

$registrypasswords = az acr credential show -n "galleryazmregistry"
$registrypasswords = [System.String]::Join("", $registrypasswords)
$registrypasswords = ConvertFrom-Json -inputobject $registrypasswords
$registrypassword = $registrypasswords.passwords[0].value

docker login galleryazmregistry.azurecr.io -u "galleryazmregistry" -p $registrypassword

# Tag containers
$containerVersion = [System.DateTime]::Now.ToString("yyyyMMddHHmmss")
docker tag photogalleryapp:latest galleryazmregistry.azurecr.io/photogalleryapp:$containerVersion
docker tag photogalleryworker:latest galleryazmregistry.azurecr.io/photogalleryworker:$containerVersion

# Push containers
docker push galleryazmregistry.azurecr.io/photogalleryapp:$containerVersion
docker push galleryazmregistry.azurecr.io/photogalleryworker:$containerVersion

# Create Linux App Service plan
az appservice plan create -n "gallery-plan-linux" -g $resourceGroupName2 -l WestEurope --sku S1 --is-linux

# Create Linux Web App
az webapp create -g $resourceGroupName2 -p "gallery-plan-linux" -n "gallery-app-linux" -i galleryazmregistry.azurecr.io/photogalleryapp:$containerVersion

# TODO: set connection strings


## PART 5

# Setup Kubernetes cluster
az aks get-versions -l WestEurope 
az aks create -g $resourceGroupName2 -l WestEurope -n "galleryazmkube" --node-count 1 --node-vm-size Standard_B2ms --node-osdisk-size 32 --generate-ssh-keys --kubernetes-version 1.14.8

# Map kubectl to Azure cluster
az aks get-credentials -g $resourceGroupName2 -n "galleryazmkube" --overwrite-existing

# Set permissions for portal
kubectl create clusterrolebinding kubernetes-dashboard -n kube-system --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard

# Set secrets for container registry and application
kubectl create secret docker-registry "galleryazmregistry" --docker-server galleryazmregistry.azurecr.io --docker-username "galleryazmregistry" --docker-password $registrypassword --docker-email herceg@vbnet.cz
kubectl apply -f deploy_secrets.yaml

# Deploy application
(get-content -path deploy_app.yaml -raw).Replace("{VERSION}", $containerVersion) | kubectl apply -f -
(get-content -path deploy_worker.yaml -raw).Replace("{VERSION}", $containerVersion) | kubectl apply -f -

# Launch portal
az aks browse -g $resourceGroupName2 -n "galleryazmkube"