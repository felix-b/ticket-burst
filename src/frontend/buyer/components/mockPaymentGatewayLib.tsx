import React from "react";
import { FormEventHandler, useState } from "react";
import {
    Flex,
    Heading,
    Input,
    Button,
    InputGroup,
    Stack,
    InputLeftElement,
    chakra,
    Box,
    Link,
    Avatar,
    FormControl,
    FormHelperText,
    InputRightElement,
    FormLabel,
    FormErrorMessage,
    GridItem,
    SimpleGrid,
    Tag,
    TagLabel,
    VStack,
    Select,
    StatGroup
} from "@chakra-ui/react";
import { Field, Form, Formik } from "formik";
import { OrderContract } from "../contracts/backendApi";
import { ServiceClient } from "../serviceClient";

export interface MockPaymentGatewayAPI {
    confirmPayment(paymentToken: string): Promise<void>
    getCustomerSession(sessionId: string): Promise<CustomerSessionData>
}

export interface CustomerSessionData {
    orderNumber: number;
    customerEmail: string;
    customerName: string;
    amount: number;
    currency: string;
    notificationStatus: string;
}

export const createMockPaymentGatewayAPI: () => MockPaymentGatewayAPI = () => {
    return {
        async confirmPayment(paymentToken: string) {
            console.log('createMockPaymentGatewayAPI.confirmPayment', paymentToken)
            const response = await fetch(`${ServiceClient.getServiceUrl('checkout')}/payment-mock/confirm-payment`, {
                method: 'POST', 
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(paymentToken),
            })
            if (response.status === 200) {
                window.top.location.href = '/success?sessionId=' + paymentToken
            }
        },
        async getCustomerSession(sessionId: string) {
            const requestUrl = `${ServiceClient.getServiceUrl('checkout')}/payment-mock/get-session?sessionId=${sessionId}`
            const response = await fetch(requestUrl)
            if (response.status === 200) {
                const envelope = await response.json()
                const data = envelope.data as CustomerSessionData
                return data
            }
            return undefined
        }
    }
}

export const MockCreditCardDetailsForm = (props: { 
    order: OrderContract
    onValidationResult: (isValid: boolean, api: MockPaymentGatewayAPI) => void
}) => {

    const { order, onValidationResult } = props
    const formikRef = React.useRef(null);
    // React.useEffect(() => {
    //     onInjectAPI({
    //         confirmPayment() {
    //             beginProcessingPayment()
    //         }
    //     })
    // })


    function validateCCNumber(value: string): string | undefined {
        return value.length === 16 ? undefined : 'Please enter 16 digits'
    }

    function validateExpiryMM(value: string): string | undefined {
        return undefined
    }

    function validateExpiryYY(value: string): string | undefined {
        return undefined
    }

    function validateCVC(value: string): string | undefined {
        return value.length === 3 ? undefined : 'Please enter 3 digits'
    }

    const onDigitOnlyInput = e => {
        e.target.value = e.target.value.replace(/[^0-9+]/g, '')
        window.setTimeout(async () => {
            const form = formikRef.current
            await form.validateForm()
            console.log('MockCreditCardDetailsForm> validaateForm>', form.isValid)
            onValidationResult(form.isValid, createMockPaymentGatewayAPI())
        })
    }

    return (
        <StatGroup border={'1px solid lightgray'} p={4} borderRadius={'2xl'} maxW='450px' mt='10px' mb='10px' color='gray.500'>
            <Formik
                innerRef={formikRef}
                initialValues={{ ccnumber: '', expirymm: '04', expiryyy: '2022', cvc: '' }}
                validateOnChange={true}
                onSubmit={(values, actions) => {
                    actions.setSubmitting(false)
                }}
            >
                {(props) => (
                    <Form>
                        <SimpleGrid columns={3} columnGap={1} rowGap={1}>
                            <GridItem colSpan={3}>
                                <Field name='ccnumber' validate={validateCCNumber}>
                                    {({ field, form, handleChange }) => (
                                        <FormControl isInvalid={form.errors.ccnumber && form.touched.ccnumber}>
                                            <FormLabel htmlFor='ccnumber' textAlign='center'>Credit Card Details</FormLabel>
                                            <Input {...field} id='ccnumber' placeholder='Card Number' textAlign='right' maxLength={16} onInput={onDigitOnlyInput} />
                                        </FormControl>
                                    )}
                                </Field>
                            </GridItem>
                            <GridItem colSpan={1}>
                                <Field name='expirymm' validate={validateExpiryMM}>
                                    {({ field, form, handleChange }) => (
                                        <FormControl isInvalid={form.errors.expirymm && form.touched.expirymm}>
                                            <Select {...field}  textAlign='right' id='expirymm'>
                                                <option value='01'>01</option>
                                                <option value='02'>02</option>
                                                <option value='03'>03</option>
                                                <option value='04'>04</option>
                                                <option value='05'>05</option>
                                                <option value='06'>06</option>
                                                <option value='07'>07</option>
                                                <option value='08'>08</option>
                                                <option value='09'>09</option>
                                                <option value='10'>10</option>
                                                <option value='11'>11</option>
                                                <option value='12'>12</option>
                                            </Select>
                                            <FormLabel textAlign='right' htmlFor='expirymm'>Expiry Month</FormLabel>
                                        </FormControl>
                                    )}
                                </Field>
                            </GridItem>
                            <GridItem colSpan={1}>
                                <Field name='expiryyy' validate={validateExpiryYY}>
                                    {({ field, form, handleChange }) => (
                                        <FormControl isInvalid={form.errors.expiryyy && form.touched.expiryyy}>
                                            <Select {...field} id='expiryyy'>
                                                <option value='2022'>2022</option>
                                                <option value='2023'>2023</option>
                                                <option value='2024'>2024</option>
                                                <option value='2025'>2025</option>
                                                <option value='2026'>2026</option>
                                                <option value='2027'>2027</option>
                                            </Select>
                                            <FormLabel htmlFor='expiryyy'>Year</FormLabel>
                                        </FormControl>
                                    )}
                                </Field>
                            </GridItem>
                            <GridItem colSpan={1}>
                                <Field name='cvc' validate={validateCVC}>
                                    {({ field, form, handleChange }) => (
                                        <FormControl isInvalid={form.errors.cvc && form.touched.cvc}>
                                            <Input {...field} id='cvc' textAlign='right' placeholder='CVC' maxLength={3} onInput={onDigitOnlyInput} color='black'/>
                                            <FormLabel textAlign='right' htmlFor='cvc'>Number on Back</FormLabel>
                                        </FormControl>
                                    )}
                                </Field>
                            </GridItem>
                        </SimpleGrid>
                    </Form>
                )}
            </Formik>
        </StatGroup>
    )
}
