$resourceGroupName = "speaking-cloud-native"
$resourceGroupName2 = "speaking-cloud-native2"
$subscriptionName = "Subscription 19"
$accountName = "rigantigallerydemo"

# Login
az login
az account set --subscription $subscriptionName

# Create resource group
az group create -n $resourceGroupName -l WestEurope 
az group create -n $resourceGroupName2 -l WestEurope 



## PART 1

# Create Cosmos DB instance
az cosmosdb create --name $accountName --resource-group $resourceGroupName --enable-free-tier true --enable-public-network true --locations regionname=WestEurope 

az cosmosdb sql database create --account-name $accountName --resource-group $resourceGroupName --name photogallery --throughput 400
az cosmosdb sql container create --account-name $accountName --resource-group $resourceGroupName --database-name photogallery --name galleries --partition-key-path /id
az cosmosdb sql container create --account-name $accountName --resource-group $resourceGroupName --database-name photogallery --name photos --partition-key-path /galleryId

# Create Azure Storage

az storage account create --name $accountName --resource-group $resourceGroupName --location westeurope --sku Standard_LRS


## PART 2

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



## PART 3

# Setup Kubernetes cluster
az aks get-versions -l WestEurope 
az aks create -g $resourceGroupName2 -l WestEurope -n "galleryazmkube" --node-count 1 --node-vm-size Standard_B2ms --node-osdisk-size 32 --generate-ssh-keys --kubernetes-version 1.19.3

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