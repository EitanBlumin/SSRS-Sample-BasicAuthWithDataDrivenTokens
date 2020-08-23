# How to Enable Basic Authentication in SSRS

Edit the `RSReportServer.config` configuration file which is located in the `ReportServer` folder in the SSRS installation path.

Example locations:

- `<drive>:\Program Files\Microsoft SQL Server Reporting Services\SSRS\ReportServer`
- `<drive>:\Program Files\Microsoft SQL Server\MSRS13.MSSQLSERVER\Reporting Services\ReportServer`

Find the `<Authentication>` element and change it to something like this:

```xml
<Authentication>
	<AuthenticationTypes>
		<RSWindowsBasic>
			<LogonMethod>3</LogonMethod>
			<Realm></Realm>
			<DefaultDomain></DefaultDomain>
		</RSWindowsBasic>
		<RSWindowsNTLM/>
	</AuthenticationTypes>
	<RSWindowsExtendedProtectionLevel>Off</RSWindowsExtendedProtectionLevel>
	<RSWindowsExtendedProtectionScenario>Proxy</RSWindowsExtendedProtectionScenario>
	<EnableAuthPersistence>true</EnableAuthPersistence>
</Authentication>
```

Save the file and then restart the Report Server service.

This will enable **both** Basic Authentication, as well as the default NTLM Authentication.

## See Also

- [Configure Basic Authentication on the Report Server | Microsoft Docs](https://docs.microsoft.com/en-us/sql/reporting-services/security/configure-basic-authentication-on-the-report-server)

