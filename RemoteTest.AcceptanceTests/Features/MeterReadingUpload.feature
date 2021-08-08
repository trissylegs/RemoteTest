Feature: MeterReadingUpload

    Scenario: Uploading valid data results in a HTTP OK response

        When POST request is made to "/meter-reading-uploads" with:
        """
        AccountId,MeterReadingDateTime,MeterReadValue
        2344,22/04/2019 09:24,01002
        """
        Then the response code should be 200

    Scenario: Uploading valid row results with entry in database
        Given table "MeterReadings" is empty
        When POST request is made to "/meter-reading-uploads" with:
        """
        AccountId,MeterReadingDateTime,MeterReadValue
        2344,22/04/2019 09:24,01002
        """
        Then table "MeterReadings" has 1 rows    
        
    Scenario: Uploading valid row results with correct data
        Given table "MeterReadings" is empty
        When POST request is made to "/meter-reading-uploads" with:
        """
        AccountId,MeterReadingDateTime,MeterReadValue
        2344,22/04/2019 09:24,01002
        2233,22/04/2019 12:25,00323
        8766,22/04/2019 12:25,03440
        2344,22/04/2019 12:25,01002
        2345,22/04/2019 12:25,45522
        """
        Then table "MeterReadings" has data:
          | AccountId | MeterReadingDateTime | MeterReadValue |
          | 2344      | 2019-04-22 09:24:00  | 1002           |
          | 2233      | 2019-04-22 12:25:00  | 323            |
          | 8766      | 2019-04-22 12:25:00  | 3440           |
          | 2344      | 2019-04-22 12:25:00  | 1002           |
          | 2345      | 2019-04-22 12:25:00  | 45522          |          
          


    Scenario: Excess whitespace in dates.
        Given table "MeterReadings" is empty
        When POST request is made to "/meter-reading-uploads" with:
        """
        AccountId,MeterReadingDateTime,MeterReadValue
        2344,  22/04/2019 09:24,01002
        2345,22/04/2019 09:24  ,01002
        2346,22/04/2019   09:24,01002
        """
        Then table "MeterReadings" has data:
          | AccountId | MeterReadingDateTime | MeterReadValue |
          | 2344      | 2019-04-22 09:24:00  | 1002           |
          | 2345      | 2019-04-22 09:24:00  | 1002           |
          | 2346      | 2019-04-22 09:24:00  | 1002           |

    Scenario: Conflicting rows are rejected
        Given table "MeterReadings" is empty
        When POST request is made to "/meter-reading-uploads" with:
        """
        AccountId,MeterReadingDateTime,MeterReadValue
        2344,22/04/2019 09:24,01002
        2344,22/04/2019 09:24,01003
        """
        Then table "MeterReadings" has data:
          | AccountId | MeterReadingDateTime | MeterReadValue |
          | 2344      | 2019-04-22 09:24:00  | 1002           |
          And Response Body JSON is:
          """
          {"successful": 1, "failed": 1}
          """
