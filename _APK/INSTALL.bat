REM #**********************
REM # REMEMBER TO PUSH THE KEY IN ORDER FOR THE APP TO ACCESS THE SERVER
REM #**********************
adb devices
REM #**********************
REM # TECH RACE BARCELONA
REM #**********************
adb shell am force-stop com.yourvrexperience.yourbitcoinmanager
adb uninstall com.yourvrexperience.yourbitcoinmanager
adb install BitcoinManager.apk
pause