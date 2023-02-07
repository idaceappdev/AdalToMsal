---
languages:
  - .net core
  - .net framework
  - javascript
  - python
products:
  - adal
  - msal
  - azure-active-directory
page_type: sample
urlFragment: ms-identity-msal-migration-samples
description: "Code samples demonstrating ADAL to MSAL migration on different platforms"
---

# Code samples demonstrating ADAL to MSAL migration on different platforms

## Background

If any of your applications use the Azure Active Directory Authentication Library (ADAL) for authentication and authorization functionality, it's time to migrate them to the Microsoft Authentication Library (MSAL).

  - All Microsoft support and development for ADAL, including security fixes, ends in June 2023.
  - There are no ADAL feature releases or new platform version releases planned prior to June 2023.
  - No new features have been added to ADAL since June 30, 2020.

The Microsoft Authentication Library (MSAL) enables developers to acquire tokens from the Microsoft identity platform, allowing applications to authenticate users and access secured web APIs. It can be used to provide secure access to Microsoft Graph, other Microsoft APIs, third-party web APIs, or your own web API.

MSAL leverages all the benefits of Microsoft identity platform (v2.0) endpoint. Check out our endpoint comparison doc for more details,
https://learn.microsoft.com/en-us/azure/active-directory/azuread-dev/azure-ad-endpoint-comparison

## About these samples

This collection of samples covers a number of scenarios demonstrating the step by step guidelines to follow when migrating an existing ADAL-based application over to MSAL with the least amount of changes. We have categorized the samples into different buckets based on the underlying platform and language used. Each of those buckets will further contain a "Before migration" sample which shows the application using ADAL library, and an "After migration" sample which shows the code changes needed to upgrade the same sample to MSAL. 

It also includes a separate section ("IdentifyingADALApps") that can help tenant admins identify ADAL apps in a tenant using our workbooks solution and/or KQL queries.

## Migration help

If you have questions about migrating your app from ADAL to MSAL, here are some options:

  1. Post your question on Microsoft Q&A and tag it with [azure-ad-adal-deprecation].
  2. Open an issue in the library's GitHub repository. See the Languages and frameworks section of the MSAL overview article for links to each library's repo.

If you partnered with an Independent Software Vendor (ISV) in the development of your application, we recommend that you contact them directly to understand their migration journey to MSAL.

## Reference

Check out our ADAL-MSAL migration guide here for different application types here:
https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-migration
