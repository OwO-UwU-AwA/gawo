#!/bin/bash

# Change Back
PORT="5001"
FILE="test.db"

# Start Database With Default Credentials
surreal start -A -u root -p root -b 0.0.0.0:"${PORT}" file:"${FILE}" &
PID="${!}"

sleep 5

# Get Auth Token
TOKEN=$(curl -X POST -H "Accept: application/json" -d '{"user":"root","pass":"root"}' http://localhost:"${PORT}"/signin | jq -r .token || exit "NODB")

# Create Users Table
curl -X POST -u "root:root" -H "NS: main" -H "DB: main" -H "Accept: application/json" -d "BEGIN TRANSACTION; DEFINE TABLE Users SCHEMAFULL; DEFINE FIELD absence ON TABLE Users TYPE int; DEFINE FIELD class ON TABLE Users TYPE string; DEFINE FIELD email ON TABLE Users VALUE string::lowercase(\$value) ASSERT string::is::email(\$value) TYPE string; DEFINE FIELD firstname ON TABLE Users TYPE string; DEFINE FIELD lastname ON TABLE Users TYPE string; DEFINE FIELD masterpassword ON TABLE Users TYPE string; DEFINE FIELD password ON TABLE Users TYPE string; DEFINE FIELD permissions ON TABLE Users TYPE int; DEFINE FIELD username ON TABLE Users TYPE string; COMMIT TRANSACTION;" http://localhost:"${PORT}"/sql

# Create Events Table
curl -X POST -u "root:root" -H "NS: main" -H "DB: main" -H "Accept: application/json" -d "BEGIN TRANSACTION; DEFINE TABLE Events SCHEMAFULL; DEFINE FIELD accepted ON TABLE Events TYPE bool; DEFINE FIELD capacity ON TABLE Events TYPE int ASSERT \$value >= 0; DEFINE FIELD created ON TABLE Events TYPE datetime; DEFINE FIELD date ON TABLE Users TYPE datetime; DEFINE FIELD description ON TABLE Events TYPE string; DEFINE FIELD duration ON TABLE Events TYPE int; DEFINE FIELD grades ON TABLE Events TYPE int; DEFINE FIELD name ON TABLE Events TYPE string; DEFINE FIELD notes ON TABLE Events TYPE string; DEFINE FIELD organiser ON TABLE Events TYPE record<Users>; DEFINE FIELD picture ON TABLE Events TYPE string; DEFINE FIELD room ON TABLE Events TYPE string; DEFINE FIELD subject ON TABLE Events TYPE string ASSERT \$value INSIDE ['ART', 'ENGLISH', 'FRENCH', 'GERMAN', 'LATIN', 'RUSSIAN', 'ASTRONOMY', 'BIOLOGY', 'COMPUTERSCIENCE', 'MATHS', 'PHYSICS', 'GEOGRAPHY', 'HISTORY', 'POLITICS', 'MISC', 'SPORT', 'STUDIESCONSULTING']; DEFINE FIELD teacher ON TABLE Events TYPE record<Users> ASSERT array::at((SELECT permissions FROM Users WHERE id = type::thing(\$value)), 0).permissions INSIDE [3, 4]; DEFINE FIELD type ON TABLE Events TYPE string ASSERT \$value INSIDE ['PRESENTATION', 'GPRESENTATION', 'FLANGPRESENTATION', 'THESISDEF', 'COMPETITION', 'WORKSHOP', 'QF', 'SPORT', 'ELMOS', 'OTHER']; COMMIT TRANSACTION;" http://localhost:"${PORT}"/sql


# Create GaWo Table
curl -X POST -u "root:root" -H "NS: main" -H "DB: main" -H "Accept: application/json" -d "BEGIN TRANSACTION; DEFINE TABLE GaWo SCHEMAFULL; DEFINE FIELD gawophase ON TABLE GaWo TYPE duration; DEFINE FIELD registrationphase ON TABLE GaWo TYPE duration; DEFINE FIELD reservationphase ON TABLE GaWo TYPE duration; DEFINE FIELD seachphase ON TABLE GaWo TYPE duration; COMMIT TRANSACTION;" http://localhost:"${PORT}"/sql

# Create VerificationLinks Table
curl -X POST -u "root:root" -H "NS: main" -H "DB: main" -H "Accept: application/json" -d "BEGIN TRANSACTION; DEFINE TABLE VerificationLinks SCHEMAFULL; DEFINE FIELD secret ON TABLE VerificationLinks TYPE string; DEFINE FIELD type ON TABLE VerificationLinks TYPE string ASSERT \$value == 'email' OR \$value == 'password'; DEFINE FIELD user ON TABLE VerificationLinks TYPE record<Users>; COMMIT TRANSACTION;" http://localhost:"${PORT}"/sql


# Setup User
curl -X POST -u "root:root" -H "NS: main" -H "DB: main" -H "Accept: application/json" -d "BEGIN TRANSACTION; DEFINE USER surrealUser ON NAMESPACE PASSWORD 'SurrealGaWo13' ROLES OWNER; COMMIT TRANSACTION;" http://localhost:"${PORT}"/sql

kill -9 "${PID}"
