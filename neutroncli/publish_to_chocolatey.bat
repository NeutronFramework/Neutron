set version=0.2.6

choco push build\artifacts\neutroncli.%version%.nupkg --source https://push.chocolatey.org/ --api-key %CHOCOLATEY_API_KEY%