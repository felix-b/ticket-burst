#!/bin/bash

aws stepfunctions create-state-machine \
  --name ticketburst-web-checkout-statemachine \
  --definition "$(cat /Users/felixbrm/poc/ticket-burst/src/backend/TicketBurst.CheckoutService/Integrations/StepFunctions/checkout-state-machine.asl.json)" \
  --role-arn 'arn:aws:iam::381777116710:role/eksctl-ticketburst-cluster-nodegr-NodeInstanceRole-1CPC35X2A3FO' \
  --type 'STANDARD' \
  --tracing-configuration enabled=true
  --logging-configuration='{"level":"ALL", "includeExecutionData":true, "destinations":[{"cloudWatchLogsLogGroup":{"logGroupArn":"arn:aws:logs:eu-south-1:381777116710:log-group:ticketburst-checkoutworkflow-loggroup:*"}}]}'


aws stepfunctions update-state-machine \
  --state-machine-arn 'arn:aws:states:eu-south-1:381777116710:stateMachine:ticketburst-web-checkout-statemachine' \
  --definition "$(cat /Users/felixbrm/poc/ticket-burst/src/backend/TicketBurst.CheckoutService/Integrations/StepFunctions/checkout-state-machine.asl.json)" \
  --tracing-configuration enabled=true \
