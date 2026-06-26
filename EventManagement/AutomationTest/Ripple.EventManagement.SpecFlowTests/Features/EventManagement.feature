Feature: Event management API automation tests
  The Event Management API should protect event operations, validate input, and persist event data.

Scenario: Event manager creates an event successfully
  Given I am authenticated as an "EventManager"
  When I create an event with the following values
    | name                 | description              | venue              | capacity | tierName | tierPrice |
    | UPSA Championship    | National championship    | Wintrust Complex   | 500      | General  | 25.00     |
  Then the response status code should be 201
  And the event response should contain "UPSA Championship"
  And the created event should be available by id

Scenario: Customer can view events
  Given an event exists named "Summer Showcase"
  And I am authenticated as a "Customer"
  When I request all events
  Then the response status code should be 200
  And the event list should contain "Summer Showcase"

Scenario: Anonymous user cannot view events
  Given I am not authenticated
  When I request all events
  Then the response status code should be 401

Scenario: Customer cannot create an event
  Given I am authenticated as a "Customer"
  When I create an event with the following values
    | name              | description      | venue            | capacity | tierName | tierPrice |
    | Restricted Event  | Should be denied | Wintrust Complex | 100      | General  | 10.00     |
  Then the response status code should be 403

Scenario: Create event request requires a valid payload
  Given I am authenticated as an "EventManager"
  When I create an invalid event with no name and zero capacity
  Then the response status code should be 400

Scenario: Event manager updates an existing event
  Given an event exists named "Original Event"
  And I am authenticated as an "EventManager"
  When I update the event name to "Updated Event"
  Then the response status code should be 204
  And the event response should contain "Updated Event"

Scenario: Event manager deletes an existing event
  Given an event exists named "Event To Delete"
  And I am authenticated as an "EventManager"
  When I delete the event
  Then the response status code should be 204
  And requesting the event by id should return 404
