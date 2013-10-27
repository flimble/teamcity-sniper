function Get-Default-Sql-Instance
{
  $instances = (get-itemproperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server').InstalledInstances
  if($instances.count -gt 0) {
  	$result = $instances[0]

  	if($result -eq "MSSQLSERVER") {
  		return "(local)"
  	}
  	else {
    	return $instances[0]
    }
  }
  else {
    return $null
  }
}

function Get-File-Exists-On-Path
{
	param(
		[string]$file
	)
	$results = ($Env:Path).Split(";") | Get-ChildItem -filter $file -erroraction silentlycontinue
	$found = ($results -ne $null)
	return $found
}

function Get-Git-Commit
{
	if ((Get-File-Exists-On-Path "git.exe")){
		$gitLog = git log --oneline -1
		return $gitLog.Split(' ')[0]
	}
	else {
		return "0000000"
	}
}

function Verify-Net-45-Installed {

	if( (ls "$env:windir\Microsoft.NET\Framework\v4.0*") -eq $null ) {
		throw ".Net 4.0 install directory cannot be found on windows path"
	}

	$version = (get-itemproperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full').Version
  	if(! $version.StartsWith("4.5")) {
  		throw ".NET 4.5 not found in registry"
  	}
}

function Set-ConfigAppSetting
    ([string]$PathToConfig=$(throw 'Configuration file is required'),
         [string]$Key = $(throw 'No Key Specified'), 
         [string]$Value = $(throw 'No Value Specified'),
         [Switch]$Verbose,
         [Switch]$Confirm,
         [Switch]$Whatif)
{
    $AllAnswer = $null
    if (Test-Path $PathToConfig)
    {
    		Write-Host "updating $Key in config $PathToConfig "
            $x = [xml] (type $PathToConfig)
            $node = $x.configuration.SelectSingleNode("appSettings/add[@key='$Key']")
            $node.value = $Value
            $x.Save($PathToConfig)
       
    }
} 


function Get-ConfigAppSetting
([string]$PathToConfig=$(throw 'Configuration file is required'))
{
    if (Test-Path $PathToConfig)
    {
        $x = [xml] (type $PathToConfig)
        $x.configuration.appSettings.add
    }
    else
    {
        throw "Configuration File $PathToConfig Not Found"
    }
}

function Roundhouse-Kick-Database 
([string]$DatabaseName=$(throw 'DatabaseName is required'),
 [string]$TargetServer=$(throw 'TargetServer is required'),
 [string]$Environment=$(throw 'Environment is required'),
 [bool]$UseSqlAuthentication=$false,
 [string]$LoginUser="",
 [string]$LoginPassword="",
 [bool]$DropCreate=$true,
 [bool]$RestorefromBackup=$false,
 [string]$BackupFile="")
{ 
	$SqlFilesDirectory = "$DatabaseName.Database"
	$RepositoryPath="$/SAIGPS Team Project/SAIGPS/Trunk"

	$args = @()

	if($RestorefromBackup -eq $true) {
		$args += @("--restore",
		"--restoretimeout=9000",
		"--commandtimeoutadmin=9000",
		"--restorefrom=$BackupFile")
	}

	if($UseSqlAuthentication -eq $true) {
		$args += @("--connectionstring=server=$TargetServer;database=$DatabaseName;uid=$LoginUser;pwd=$LoginPassword")
	}





	if($DropCreate -eq $true) {
		exec { roundhouse\console\rh.exe --servername=$TargetServer --database=$DatabaseName --noninteractive --drop }
	}


	exec { roundhouse\console\rh.exe --servername=$TargetServer --database=$DatabaseName --environment=$Environment --sqlfilesdirectory=$SqlFilesDirectory --repositorypath=$RepositoryPath --upfolder="2.Tables and Data (MODIFICATIONS WILL REQUIRE DATABASE REFRESH)" --runfirstafterupdatefolder="3.Synonyms (MODIFICATIONS WILL REQUIRE DATABASE REFRESH)" --functionsfolder="4.Functions (DROP CREATE)" --viewsfolder="5.Views (DROP CREATE)" --sprocsfolder="6.Stored Procedures (DROP CREATE)" --indexesfolder="7.Indexes (DROP CREATE)" --runafterotheranytimescriptsfolder="8.Environment Configuration Data" --permissionsfolder="9.SQL Server Permissions" --noninteractive --commandtimeout=1200 $args }
}


function Install-Nuget-Packages([string]$PackagesDirectory=$(throw 'Target packages directory is required')) { 
	$nuget_exe = '.nuget\nuget.exe'

	if( (Test-Path $nuget_exe) -eq $false ) {
		throw "nuget.exe cannot be found on path"
	}

	$configs = Get-ChildItem -filter "packages.config" -recurse
	

	foreach($config in $configs)
	{
		$fullname = $config.fullname

		exec { & $nuget_exe install "$fullname" -o  "$packages_dir" }
	}
}

function Generate-Environment-Config([string]$config=$(throw 'Config path is required'),
	[string]$applicationName=$(throw 'application name is required'),
	[string]$environmentName=$(throw 'environment name is required')) 
{
	
}


