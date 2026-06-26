Feature: Ticket inventory management
  As a ticket agent
  I want to create event inventory records
  So that ticket capacity can be managed before sales begin

  Background:
    Given the event catalog contains an event named "Summer Showcase" with 20 total tickets and a "VIP" price tier costing 40.00

  Scenario: Ticket agent creates inventory successfully
    When a "TicketAgent" creates inventory with 20 total tickets
    Then the response status code should be 201
    When a "Customer" checks ticket availability
    Then the response status code should be 200
    And the availability should show 20 tickets remaining

  Scenario: Customer cannot create inventory
    When a "Customer" creates inventory with 20 total tickets
    Then the response status code should be 403
