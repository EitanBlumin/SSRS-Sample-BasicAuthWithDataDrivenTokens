# Enabling Cross-Origin Reference in SSRS

If you want to reference your SSRS reports using an `iframe` or `HttpWebRequest` from a website that is located on a different host, then you'll need to do the following:

First, you need to enable CORS in SSRS.
[Click here for instructions](activating_cors_ssrs.md).

If you're using a **Custom Security Extension**, then you'll need to enable the `SameSite=None` and `Secure=True` flags for the SSRS cookies.

1. Edit the `web.config` file in the SSRS installation folder and make sure you have the following:

```xml
<configuration>
 <system.web>
  <httpCookies sameSite="None" requireSSL="true" />
  <anonymousIdentification cookieRequireSSL="true" />
  <authentication>
   <forms cookieSameSite="None" requireSSL="true" />
  </authentication>
  <sessionState cookieSameSite="None" />
  <roleManager cookieRequireSSL="true" />
 <system.web>
<configuration>
```

2. Since you're setting the `Secure=true` flag, this means that **your Report Server must now be accessed with SSL enabled**.
3. You'll need to enable SSL for both your report server and also your referencing web host.

