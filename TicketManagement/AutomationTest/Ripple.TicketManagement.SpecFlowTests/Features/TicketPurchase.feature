Feature: Ticket purchase
  As a ticketing API client
  I want to purchase tickets through the Ticket Management API
  So that inventory and sales summaries are updated correctly

  Background:
    Given the event catalog contains an event named "UPSA Championship" with 10 total tickets and a "General" price tier costing 25.00

  Scenario: Customer purchases tickets successfully
    When a "Customer" purchases 2 tickets for "buyer@example.com"
    Then the response status code should be 200
    And the ticket order should contain 2 tickets at 25.00 each
    When a "Customer" checks ticket availability
    Then the response status code should be 200
    And the availability should show 8 tickets remaining

  Scenario: API prevents overselling inventory
    When a "Customer" purchases 11 tickets for "buyer@example.com"
    Then the response status code should be 400
    And the error response should contain "Not enough tickets available"

  Scenario: Purchase request requires a valid customer role
    When an unauthenticated user purchases 1 tickets for "buyer@example.com"
    Then the response status code should be 401

  Scenario: Invalid purchase request is rejected by validation
    When a "Customer" purchases 0 tickets for "not-an-email"
    Then the response status code should be 400
    And the error response should contain "Purchaser Email"

  Scenario: Ticket agent can view sales summary
    Given a "Customer" already purchased 3 tickets for "buyer@example.com"
    When a "TicketAgent" requests the sales summary
    Then the response status code should be 200
    And the sales summary should show 1 orders, 3 tickets sold, and 75.00 gross sales
