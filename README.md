# Project2_fingerprint
This project is a simple application written in C#, using an U.Are.U SDK for fingerprint verification.
Data enrollment is performed a fingerprint scanner. All the fingerprints will be stored as bitmap image files (*.bmp)

### Prerequisites

In order to run the application, the following program and SDK should be installed:
1. Visual Studio 2010 or higher - for running the project code
2. U.Are.U SDK - https://www.crossmatch.com/biometric-identity-solutions/products/software/sdk/

## Setting error threshold
When a fingerprint is scanned, the first step is to identify the fingerprint against a set of stored FMDs.
Markup : * If you are trying to confirm that a user is allowed access, you will want to identify the fingerprint against all
the valid FMDs that you have stored.
Markup : * If you are trying to confirm the identity of a specific person, you must identify against all FMDs for that
individual (typically at least two fingers are stored for each user).
The identification function compares an FMV against a collection of FMDs to produce the candidate list. You
can specify the maximum desired number of candidates: a smaller number can make the execution faster. The
most similar candidates are returned closer to the beginning of the candidate list.

Your threshold determines the trade-off between false positive and false negative error rates where:
Markup : * 0 = no false positives
Markup : * maxint (#7FFFFFFF or 2147483647) = fingerprints do not match at all
Markup : * Values close to 0 allow very few false positives; values closer to maxint allow very poor matches (a lot of
false positives) in the candidate list. The table below shows the ralationship between the threshold values
and the false positive identification error rates observed in our test. Note: the actual false positive
identification error rates in your deployment may vary.

![alt text](https://user-images.githubusercontent.com/35852207/50937855-99f75b80-1443-11e9-80b1-6dfb8c98cb7b.PNG)

For many applications, a good starting point for testing is a threshold of 1 in 100,000. If you want to be
conservative, then you will want to set the threshold lower than the desired error rate (e.g., if you want an error
rate that does not exceed 1 in 100,000, you might set the threshold to 1 in 1,000,000)

## Running the tests
For convinence, the database should be organized as follows:
1. Root directory
2. Sub-directories named after the person whose fingerprint were enrolled in the database

Below are the steps for using the application for fingerprint verification
1. Provide a path to data folder (containing fingerprints files). Note that fingerprints files should be in bitmap format so that the SDK can convert it into bytes[] for verification
2. Load the fingerprint to be verify
3. Button verify click
4. The application will create matchscore files inside each sub-directories in the database folder

Below is the flowchart as to how this application works

![alt text](https://user-images.githubusercontent.com/35852207/50934068-762d1900-1435-11e9-9cbe-1baedc503651.PNG)
![alt text](https://user-images.githubusercontent.com/35852207/50934074-775e4600-1435-11e9-9848-268411507e18.PNG)
