import { useState } from "react";
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
    HStack,
    Alert,
    AlertIcon
} from "@chakra-ui/react";
import { Field, Form, Formik } from "formik";
import { BeginCheckoutRequest, OrderContract } from "../contracts/backendApi";
import { MockCreditCardDetailsForm, MockPaymentGatewayAPI } from "./mockPaymentGatewayLib";
import { ServiceClient } from "../serviceClient";

interface PersonalDetailsFormData {
    fullName: string
    emailAddress: string
}

const PersonalDetailsForm = (props: { 
    enabled: boolean
    onCompleted: (data: PersonalDetailsFormData) => void 
}) => {

    const { enabled, onCompleted } = props

    function validateName(value: string) {
        if (!value || value.trim().length < 2) {
            return 'Name is required'
        }
    }

    function validateEmail(value: string) {
        const regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/

        if (!value || value.trim().length < 2) {
            return 'E-mail address is required'
        }
        if (value.length > 320 || !regex.test(value)) {
            return 'Invalid e-mail address'
        }
    }

    return (
        <Box w='full'>
            <Heading fontSize='2xl'>Personal Details</Heading>
            <Formik
                initialValues={{ name: '', email: '' }}
                validateOnChange={true}
                onSubmit={(values, actions) => {
                    const data: PersonalDetailsFormData = {
                        fullName: values.name,
                        emailAddress: values.email
                    }
                    actions.setSubmitting(false)
                    onCompleted(data)
                }}
            >
                {(props) => (
                    <Form aria-disabled={!enabled}>
                        <Field name='name' validate={validateName} disabled={!enabled}>
                            {({ field, form, handleChange }) => (
                                <FormControl isInvalid={form.errors.name && form.touched.name} isDisabled={!enabled}>
                                    <FormLabel htmlFor='name'>Full name</FormLabel>
                                    <Input {...field} id='name' placeholder='Full Name' />
                                    <FormErrorMessage>{form.errors.name}</FormErrorMessage>
                                </FormControl>
                            )}
                        </Field>
                        <Field name='email' validate={validateEmail}>
                            {({ field, form, handleChange }) => (
                                <FormControl isInvalid={form.errors.email && form.touched.email} isDisabled={!enabled}>
                                    <FormLabel htmlFor='email'>E-mail address</FormLabel>
                                    <Input {...field} id='email' placeholder='E-mail Address' />
                                    <FormErrorMessage>{form.errors.email}</FormErrorMessage>
                                </FormControl>
                            )}
                        </Field>
                        <Field>
                            {({ field, form }) => enabled && <Button
                                mt={4}
                                colorScheme='teal'
                                isLoading={props.isSubmitting}
                                type='submit'
                                disabled={form.errors.name || form.errors.email || !form.touched.name || !form.touched.email}
                            >
                                Continue&nbsp;&gt;
                            </Button>}
                        </Field>
                    </Form>
                )}
            </Formik>
        </Box>
    )
}

const PaymentDetailsForm = (props: { 
    orderWithPaymentToken: OrderContract,
    onStepBack: () => void
}) => {
    const [isValid, setIsValid] = useState(false)
    const [paymentAPI, setPaymentAPI] = useState<MockPaymentGatewayAPI>(null)
    const { orderWithPaymentToken, onStepBack } = props

    console.log('PaymentDetailsForm.render', 'PAYMENT-TOKEN', orderWithPaymentToken.paymentToken)

    const handleSubmit = () => {
        console.log('PaymentDetailsForm.handleSubmit', 'PAYMENT-TOKEN', orderWithPaymentToken.paymentToken)
        paymentAPI.confirmPayment(orderWithPaymentToken.paymentToken)
    }

    return (
        <Box w='full'>
            <Heading fontSize='2xl'>Secure Payment</Heading>
            <Alert status='warning' variant='subtle' fontSize='16' fontWeight={400} lineHeight='1.0' mt='10px'>
                <AlertIcon />
                Mock Payment Gateway. Do not enter real credit cards.
            </Alert>
            <MockCreditCardDetailsForm 
                order={orderWithPaymentToken} 
                onValidationResult={(result, api) => { 
                    setIsValid(result); 
                    setPaymentAPI(api); 
                }}
            />
            <HStack mt={4}>
                <Button colorScheme='teal' fontSize='xl' onClick={handleSubmit} disabled={!isValid}>Pay and Get Tickets</Button>
                <Button colorScheme='gray' onClick={onStepBack}>&lt;&nbsp;Edit Personal Details</Button>
            </HStack>
        </Box>
    )
}

export const TwoStepCheckoutForm = (props: { 
    order: OrderContract,
    eventId: string,
    areaId: string,
    checkoutToken: string
}) => {
    const { order, eventId, areaId, checkoutToken } = props
    const [personalData, setPersonalData] = useState<PersonalDetailsFormData>(null)
    const [paymentToken, setPaymentToken] = useState<string>(null)

    const enrichedOrder: OrderContract | undefined = personalData && paymentToken
        ? {
            ...props.order,
            customerName: personalData.fullName,
            customerEmail: personalData.emailAddress,
            paymentToken
        }
        : undefined

    const beginCreatePaymentIntent = async (data: PersonalDetailsFormData) => {
        const request: BeginCheckoutRequest = {
            preview: false,
            eventId,
            hallAreaId: areaId,
            checkoutToken,
            customerName: data.fullName,
            customerEmail: data.emailAddress            
        }
        const orderWithPaymentIntent = await ServiceClient.beginCheckout(request)
        console.log('TwoStepCheckoutForm> orderWithPaymentIntent>', orderWithPaymentIntent)
        setPersonalData(data)
        setPaymentToken(orderWithPaymentIntent.paymentToken)
    }

    return (
        <SimpleGrid w='full' templateColumns='30px auto' templateRows='auto auto' gap='20px'>
            <GridItem>
                <Tag size={'lg'} borderRadius='2xl' variant='solid' colorScheme='teal' verticalAlign='start'>
                    <TagLabel>1</TagLabel>
                </Tag>
            </GridItem>
            <GridItem>
                <VStack w='full' justify='stretch' justifyItems='stretch' justifyContent='stretch'>
                    <PersonalDetailsForm enabled={!personalData} onCompleted={data => beginCreatePaymentIntent(data)}/>
                </VStack>
            </GridItem>
            {paymentToken && 
                <GridItem>
                    <Tag size={'lg'} borderRadius='2xl' variant='solid' colorScheme='teal' verticalAlign='start'>
                        <TagLabel>2</TagLabel>
                    </Tag>
                </GridItem>
            }
            {paymentToken && 
                <GridItem>
                    <VStack justify='start' justifyItems='start'>
                        <PaymentDetailsForm orderWithPaymentToken={enrichedOrder} onStepBack={() => setPaymentToken(null)} />
                    </VStack>
                </GridItem>
            }
        </SimpleGrid>
    )
}
