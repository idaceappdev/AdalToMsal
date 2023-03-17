## Project Structure
		This project consists of following two folders. 
			§ active-directory-java-webapp-openidconnect-ADAL
			§ active-directory-java-webapp-openidconnect-MSAL
			
		The folder ends with ADAL contains a Java Servlet application project that uses ADAL libraries to sign-in users and acquire tokens to execute Microsoft Graph queries. The folder ends with MSAL contains an updated version of the Java Servlet application which uses MSAL libraries to sign-in and acquire tokens to execute Microsoft Graph queries
		
Please refer Readme files in respective folders for a detailed walkthrough on how to configure the projects for execution in your environments


## Pre-requisite
	
		Java 8 - https://www.oracle.com/java/technologies/downloads/#java8-windows
        	Maven -  https://maven.apache.org/download.cgi
		§ Installation Instructions: https://maven.apache.org/install.html
		Apache Tomcat -Apache Tomcat® - https://tomcat.apache.org/download-80.cgi
		(Since this is a legacy application, for compatibility, it requires Apache TomCat <= 9.x). For this demo verion 8.5 of TomCat server is used)
		
Refer the ADAL to MSAL migration guide: https://learn.microsoft.com/en-us/azure/active-directory/develop/migrate-adal-msal-java

## Migration steps 

		Follow the steps described in ADAL - MSAL Migration (Java).pdf
