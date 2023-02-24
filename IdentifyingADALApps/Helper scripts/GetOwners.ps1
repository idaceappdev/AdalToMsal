param (
    [Parameter(Mandatory)] $ApplicationIDcsv
)

function getAppInformation()
{
    param ([Parameter()] $app)

    $d = '' | Select-Object ApplicationId,DisplayName,AppOwnerOrganizationId,ApplicationObjectId,ServicePrincipalObjectId,ApplicationOwnerObjectId,ApplicationOwner,ServicePrincipalOwnerObjectId,ServicePrincipalOwner,ServiceManagementReference,ApplicationDescription,ServicePrincipalDescription
    $theFilter = "appId eq '" + $app.Appid + "'"

    $d.ApplicationId = $app.AppId

    #Get the information around the app registration
    $appReg = Get-MgApplication -Filter $theFilter
    if($null -ne $appReg)
    {
        $d.DisplayName = $appReg.DisplayName
        $d.ApplicationObjectId = $appReg.Id

        $d.ApplicationDescription = $appReg.Description
        if($null -eq $d.ApplicationDescription)
        {
            $d.ApplicationDescription = $appReg.Notes
        }
        else 
        {
            $d.ApplicationDescription = " | " + $appReg.Notes
        }

        $d.ServiceManagementReference = $appReg.ServiceManagementReference

        #check for the owners of the app registration
        $appOwners = Get-MgApplicationOwner -ApplicationId $appReg.Id
        foreach($appOwner in $appOwners)
        {
            $d.ApplicationOwnerObjectId += ($appOwner.Id + ', ')
            $d.ApplicationOwner += ((Get-MgDirectoryObject -DirectoryObjectId $appOwner.Id).AdditionalProperties["displayName"] + ', ')
        }
    }

    #Get the information around the ServicePrincipal (Enterprise Application)
    $sp = Get-MgServicePrincipal -Filter $theFilter
    if($null -ne $sp)
    {
        $d.ServicePrincipalObjectId = $sp.Id
        $d.DisplayName = $sp.DisplayName
        $d.AppOwnerOrganizationId = $sp.AppOwnerOrganizationId

        $d.ServicePrincipalDescription = $sp.Description
        if($null -eq $d.ServicePrincipalDescription)
        {
            $d.ServicePrincipalDescription = $sp.Notes
        }
        else 
        {
            $d.ServicePrincipalDescription = " | " + $sp.Notes
        }

        

        $spOwners = Get-MgServicePrincipalOwner -ServicePrincipalId $sp.Id
        foreach($spOwner in $spOwners)
        {
            $d.ServicePrincipalOwnerObjectId += ($spOwner.Id + ', ')
            $d.ServicePrincipalOwner += ((Get-MgDirectoryObject -DirectoryObjectId $spOwner.Id).AdditionalProperties["displayName"] + ', ')
        }
    }

    return $d
}

#If file exists
if(Test-Path $ApplicationIDcsv)
{

    #Import from the csv file
    $apps = Import-Csv $ApplicationIDcsv -Header "AppId" -ErrorAction Stop

    #Eliminate the header record
    $apps = $apps[1..($apps.count-1)]

    #Summary
    Write-Host "File:" $ApplicationIDcsv " - Number of apps: " $apps.Count

    #We create the unique export file path
    $exportPath = $ApplicationIDcsv.Replace(".csv","-Export-" + (Get-Date -Format "yyMMddHHmmss") + ".csv")

    #$apps = Get-MgApplication

    $data = @()
    $progress = 0

    Connect-MgGraph

    foreach($app in $apps)
    {
        $progress += 1
        $noApps = $apps.Count
        $percent = [math]::Round($progress/$noApps*100)
        Write-Progress -Activity "Get App Info" -status "AppNo: $progress of $noApps" -PercentComplete $percent -CurrentOperation "Processing...."

        $appData = getAppInformation($app)

        $data += $appData

        if($progress % 100 -eq 0)
        {
            $data | Export-Csv -Path $exportPath -NoTypeInformation -Append -NoClobber
            $data = @()
        }

    }

    $data | Export-Csv -Path $exportPath -NoTypeInformation -Append -NoClobber

    Write-Host "Source file: " $appIDcsv
    Write-Host "Results file: " $exportPath
    Write-Host "Processed: " $apps.Count
}
#If file does not exist
else 
{
    Write-Host "Issues with accessing the file."
}