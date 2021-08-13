$baseDir = (Get-Location).tostring()
$archiveDir = "$baseDir"+"\..\"
$sourceDir = "$baseDir"+"\bin\Release\"
echo "baseDir: $baseDir"
echo "archiveDir: $archiveDir"
echo "sourceDir: $sourceDir"
$compress = @{
  LiteralPath = ("$sourceDir"+"SoundVolumeView"), ("$sourceDir"+"AutoAudioSwitch.exe"), , ("$sourceDir"+"AutoAudioSwitch.ini"), ("$sourceDir"+"INIFileParser.dll"), ("$archiveDir"+"README.md")
  DestinationPath = "$archivedir"+"release.zip"
}
Compress-Archive @compress

#
