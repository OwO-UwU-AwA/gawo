# ChartAPI/

This Folder Contains Endpoints For Chart Data Which Is Fetched From The Database OnGet.

## Rules

— Each Chart Shall Have Its Own Endpoint.
— Each Chart Shall Only Use One Endpoint. No Transformations Shall Be Done On The Frontend In JavaScript.
— Each Chart Endpoint Shall Only Be Accessible Through Code By Setting The 'From' Header To 'Code' And Redirecting
To '/' If It Is Not Set.